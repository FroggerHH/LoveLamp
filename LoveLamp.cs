using System.Collections.Generic;
using UnityEngine;

namespace LoveLamp
{
    public class LoveLamp : Fireplace
    {
        [SerializeField] private float radius = 10f;
        [SerializeField] private GameObject m_areaMarker;
        internal static List<LoveLamp> all = new();

        internal void OnInteract()
        {
            ShowHideAreaMarker();
        }
        private void ShowHideAreaMarker()
        {
            if(!m_areaMarker)
                return;
            if(m_areaMarker.activeSelf) m_areaMarker.SetActive(false);
            else m_areaMarker.SetActive(true);
        }




        private static void Boost(Character character)
        {
            if(character.m_nview.GetZDO().GetBool("Boosted", false) || !character.IsTamed()) return;
            Debug.Log($"Boosting {character.m_name}");
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
                    if(!procreation.m_character.m_name.Contains("<color=yellow>Boosted</color")) procreation.m_character.m_name += " <color=yellow>Boosted</color>";

                    character.m_health *= 2;
                    character.m_walkSpeed *= 2;
                    character.m_runSpeed *= 2;
                    character.m_swimSpeed *= 2;
                    character.m_turnSpeed *= 2;
                    character.m_jumpForce *= 2;
                    character.m_level++;
                }
            }
        }
        private static void UnBoost(Character character)
        {
            if(!character.m_nview.GetZDO().GetBool("Boosted", false) || !character.IsTamed()) return;
            Debug.Log($"UnBuffing {character.m_name}");
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
                    character.m_name = character.m_name.Replace(" <color=yellow>Boosted</color>", "");

                    character.m_health /= 2;
                    character.m_walkSpeed /= 2;
                    character.m_runSpeed /= 2;
                    character.m_swimSpeed /= 2;
                    character.m_turnSpeed /= 2;
                    character.m_jumpForce /= 2;
                    character.m_level--;
                }
            }
        }
        internal static void CheckBoost(Character character)
        {
            if(HaveLoveLampInRange(character.transform.position)) Boost(character);
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


        private void OnDestroy()
        {
            all.Remove(this);
        }

        //private List<Character> buffed = new();
        /*internal void UpdatePetsInArea()
        {
            bool flag = IsBurning();
            List<Character> characters = GetCharactersInRange();

            foreach(Character character in characters)
            {
                if(!buffed.Contains(character) && flag) Boost(character);
            }
            List<Character> newBuffed = new();
            foreach(Character character1 in buffed)
            {
                newBuffed.Add(character1);
            }
            foreach(Character character1 in newBuffed)
            {
                if(!characters.Contains(character1))
                {
                    UnBoost(character1);
                }
            }
        }*/
        /*private List<Character> GetCharactersInRange()
        {
            List<Collider> colliders1 = Physics.OverlapSphere(transform.position, radius).ToList();
            List<Character> characters = new();
            foreach(Collider collider in colliders1)
            {
                if(collider.gameObject.TryGetComponent(out Character character) && (character is not Player)) characters.Add(character);
            }
            return characters;
        }*/
    }
}