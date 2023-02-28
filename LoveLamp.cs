using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LoveLamp.Plugin;

namespace LoveLamp
{
    public class LoveLamp : Fireplace
    {
        [SerializeField] internal float radius = 10f;
        [SerializeField] internal CircleProjector m_areaMarker;
        internal static List<LoveLamp> all = new();
        internal Container container;
        internal bool noFood = false;

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

        internal void ConnectChest()
        {
            Container container = GetNearestChest();
            if(!container)
            {
                Chat.instance.SetNpcText(gameObject, Vector3.up * 1.5f, 20f, 2.5f, "", "Can't find any chest near the lamp", false);
                return;
            }
            this.container = container;
            StartCoroutine(HeightlightChest(container.gameObject));
            Chat.instance.SetNpcText(gameObject, Vector3.up * 1.5f, 20f, 2.5f, "", "Chest connected", false);
        }

        private IEnumerator HeightlightChest(GameObject container)
        {
            Renderer[] componentsInChildren = container.GetComponentsInChildren<Renderer>();
            foreach(Renderer renderer in componentsInChildren)
            {
                foreach(Material material in renderer.materials)
                {
                    if(material.HasProperty("_EmissionColor"))
                        material.SetColor("_EmissionColor", Color.yellow * 0.7f);
                    material.color = Color.yellow;
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
            if(m_areaMarker.gameObject.activeSelf) m_areaMarker.gameObject.SetActive(false);
            else m_areaMarker.gameObject.SetActive(true);
        }


        private void Boost(Character character)
        {
            if(character == null || character.m_nview == null) return;
            ZDO zDO = character.m_nview.GetZDO();
            if(zDO == null) return;
            bool boosted = zDO.GetBool("Boosted", false);
            if(boosted) return;
            if(!character.IsTamed()) return;

            if(character.gameObject.TryGetComponent(out Procreation procreation))
            {
                character.m_nview.GetZDO().Set("Boosted", true);
                procreation.CancelInvoke("Procreate");
                procreation.m_updateInterval /= updateInterval;
                procreation.m_maxCreatures *= maxCreatures;
                procreation.m_requiredLovePoints /= requiredLovePoints;
                procreation.m_pregnancyChance = 1f;
                procreation.m_pregnancyDuration /= pregnancyDuration;

                procreation.InvokeRepeating("Procreate", Random.Range(procreation.m_updateInterval, procreation.m_updateInterval + procreation.m_updateInterval * 0.5f), procreation.m_updateInterval);
                if(procreation.m_tameable)
                {
                    procreation.m_tameable.m_tamingTime /= tamingTime;
                    procreation.m_tameable.m_levelUpFactor *= levelUpFactor;

                    character.m_health *= health;
                    character.m_speed *= speed;
                    character.m_jumpForce *= jumpForce;
                    character.SetLevel(character.GetLevel() + boostLevel);
                }


                BaseAI baseAI = character.GetBaseAI();
                if(!baseAI.GetPatrolPoint(out _))
                {
                    baseAI.SetPatrolPoint(transform.position);
                }
            }
        }
        internal static void UnBoost(Character character)
        {
            if(character == null || character.m_nview == null) return;
            ZDO zDO = character.m_nview.GetZDO();
            if(zDO == null) return;
            bool boosted = zDO.GetBool("Boosted", false);
            if(!boosted) return;
            if(character.GetLevel() <= 1) return;

            if(character.gameObject.TryGetComponent(out Procreation procreation))
            {
                character.m_nview.GetZDO().Set("Boosted", false);
                procreation.CancelInvoke("Procreate");
                procreation.m_updateInterval *= updateInterval;
                procreation.m_maxCreatures /= maxCreatures;
                procreation.m_requiredLovePoints *= requiredLovePoints;
                procreation.m_pregnancyChance = 0.5f;
                procreation.m_pregnancyDuration *= pregnancyDuration;

                procreation.InvokeRepeating("Procreate", Random.Range(procreation.m_updateInterval, procreation.m_updateInterval + procreation.m_updateInterval * 0.5f), procreation.m_updateInterval);
                if(procreation.m_tameable)
                {
                    procreation.m_tameable.m_tamingTime *= tamingTime;
                    procreation.m_tameable.m_levelUpFactor /= levelUpFactor;

                    character.m_health /= health;
                    character.m_speed /= speed;
                    character.m_jumpForce /= jumpForce;
                    character.m_level -= boostLevel;
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
            if(!character.m_tameable) return;
            LoveLamp loveLamp = HaveLoveLampInRange(character.transform.position);
            if(loveLamp && loveLamp.IsBurning())
            {
                loveLamp.Boost(character);
                if(character.m_tameable != null && character.m_tameable.IsHungry())
                {
                    loveLamp.GiveFood(character);
                }
            }
            else
            {
                UnBoost(character);
            }
        }

        private void GiveFood(Character character)
        {
            if(!container) return;

            ItemDrop.ItemData foodItem = null;
            Inventory inventory = container.GetInventory();
            foreach(ItemDrop.ItemData item in inventory.GetAllItems())
            {
                if(character.m_tameableMonsterAI.CanConsume(item))
                {
                    foodItem = item;
                    break;
                }
            }

            if(foodItem == null)
            {
                noFood = true;
                return;
            }
            noFood = false;


            character.m_tameableMonsterAI.m_onConsumedItem?.Invoke(character.m_tameableMonsterAI.m_consumeTarget);
            (character as Humanoid)?.m_consumeItemEffects.Create(transform.position, Quaternion.identity);
            character.m_tameableMonsterAI.m_animator.SetTrigger("consume");
            character.m_tameableMonsterAI.m_consumeTarget = null;
            inventory.RemoveOneItem(foodItem);
        }

        internal static void UnBoostAll()
        {
            foreach(Character character in Character.m_characters)
            {
                UnBoost(character);
            }
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
            List<Piece> pieces = new();
            List<Container> containers = new();
            Piece.GetAllPiecesInRadius(transform.position, chestRadius, pieces);
            foreach(Piece piece in pieces)
            {
                if(piece.gameObject.TryGetComponent(out Container container)) containers.Add(container);
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