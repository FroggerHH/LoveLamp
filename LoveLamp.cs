using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LoveLamp
{
    public class LoveLamp : Fireplace
    {
        [SerializeField] private float radius = 10f;
        [SerializeField] private GameObject m_areaMarker;
        internal static List<LoveLamp> all = new();
        internal Container container;

        internal bool OnInteract(Humanoid user, bool hold, bool shift)
        {
            if(hold) return false;
            if(shift) ConnectChest();
            else
            {
                ShowHideAreaMarker();
            }

            return true;
        }

        private void ConnectChest()
        {
            Container container = GetNearestChest();
            if(!container)
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Can't find any chest near the lamp");
                return;
            }
            this.container = container;
            StartCoroutine(HeightlightChest());
        }

        private IEnumerator HeightlightChest()
        {
            Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
            foreach(Renderer renderer in componentsInChildren)
            {
                foreach(Material material in renderer.materials)
                {
                    if(material.HasProperty("_EmissionColor"))
                        material.SetColor("_EmissionColor", Color.red * 0.7f);
                    material.color = Color.red;
                }
            }
            yield return new WaitForSeconds(1f);
            foreach(Renderer renderer in componentsInChildren)
            {
                foreach(Material material in renderer.materials)
                {
                    if(material.HasProperty("_EmissionColor"))
                        material.SetColor("_EmissionColor", Color.white * 0f);
                    material.color = Color.white;
                }
            }
        }

        private void ShowHideAreaMarker()
        {
            if(!m_areaMarker)
                return;
            if(m_areaMarker.activeSelf) m_areaMarker.SetActive(false);
            else m_areaMarker.SetActive(true);
        }




        private void Boost(Character character)
        {
            if(!character || !character.m_nview || character.m_nview.GetZDO().GetBool("Boosted", false) || !character.IsTamed() || !character.m_tameable) return;
            if(character.gameObject.TryGetComponent(out Procreation procreation))
            {
                character.m_nview.GetZDO().Set("Boosted", true);
                procreation.CancelInvoke("Procreate");
                procreation.m_updateInterval /= 2;
                procreation.m_maxCreatures *= 2;
                procreation.m_requiredLovePoints /= 2;
                procreation.m_pregnancyChance = 1f;
                procreation.m_pregnancyDuration /= 2;

                procreation.InvokeRepeating("Procreate", Random.Range(procreation.m_updateInterval, procreation.m_updateInterval + procreation.m_updateInterval * 0.5f), procreation.m_updateInterval);
                if(procreation.m_tameable)
                {
                    procreation.m_tameable.m_tamingTime /= 2;
                    procreation.m_tameable.m_levelUpFactor *= 2;

                    character.m_health *= 2;
                    character.m_walkSpeed *= 2;
                    character.m_runSpeed *= 2;
                    character.m_swimSpeed *= 2;
                    character.m_turnSpeed *= 2;
                    character.m_jumpForce *= 2;
                    character.m_level++;
                }


                BaseAI baseAI = character.GetBaseAI();
                if(!baseAI.GetPatrolPoint(out _))
                {
                    baseAI.SetPatrolPoint(transform.position);
                }
            }
        }
        private static void UnBoost(Character character)
        {
            if(!character || !character.m_nview || !character.m_nview.GetZDO().GetBool("Boosted", false) || !character.IsTamed() || !character.m_tameable) return;
            if(character.gameObject.TryGetComponent(out Procreation procreation))
            {
                character.m_nview.GetZDO().Set("Boosted", false);
                procreation.CancelInvoke("Procreate");
                procreation.m_updateInterval *= 2;
                procreation.m_maxCreatures /= 2;
                procreation.m_requiredLovePoints *= 2;
                procreation.m_pregnancyChance = 0.5f;
                procreation.m_pregnancyDuration *= 2;

                procreation.InvokeRepeating("Procreate", Random.Range(procreation.m_updateInterval, procreation.m_updateInterval + procreation.m_updateInterval * 0.5f), procreation.m_updateInterval);
                if(procreation.m_tameable)
                {
                    procreation.m_tameable.m_tamingTime *= 2;
                    procreation.m_tameable.m_levelUpFactor /= 2;

                    character.m_health /= 2;
                    character.m_walkSpeed /= 2;
                    character.m_runSpeed /= 2;
                    character.m_swimSpeed /= 2;
                    character.m_turnSpeed /= 2;
                    character.m_jumpForce /= 2;
                    character.m_level--;
                }

                BaseAI baseAI = character.GetBaseAI();
                if(!baseAI.GetPatrolPoint(out _))
                {
                    baseAI.SetPatrolPoint();
                }
            }
        }
        internal static void CheckBoost(Character character)
        {
            LoveLamp loveLamp = HaveLoveLampInRange(character.transform.position);
            if(loveLamp) loveLamp.Boost(character);
            else UnBoost(character);
        }
        public static LoveLamp HaveLoveLampInRange(Vector3 point)
        {
            foreach(LoveLamp allStation in all)
            {
                float rangeBuild = allStation.radius;
                if(Vector3.Distance(allStation.transform.position, point) < (double)rangeBuild)
                    return allStation;
            }
            return null;
        }

        private void OnDestroy() => all.Remove(this);

        private List<Container> GetChestsInRange()
        {
            List<Collider> colliders1 = Physics.OverlapSphere(transform.position, 5f).ToList();
            List<Container> containers = new();
            foreach(Collider collider in colliders1)
            {
                if(collider.gameObject.TryGetComponent(out Container container)) containers.Add(container);
            }
            return containers;
        }
        private Container GetNearestChest()
        {
            List<Container> containers = GetChestsInRange();
            Container result = null;
            float minDist = 999999;
            Vector3 currentPos = transform.position;
            foreach(Container container in containers)
            {
                float dist = Vector3.Distance(container.transform.position, currentPos);
                if(dist < minDist)
                {
                    result = container;
                    minDist = dist;
                }
            }
            return result;
        }
    }
}