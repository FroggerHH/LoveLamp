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
            StartCoroutine(HeightlightChest(container.gameObject));
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
            if(character.m_nview.GetZDO().GetBool("Boosted", false) || !character.IsTamed()) return;
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
                    character.m_level += boostLevel;
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
            if(!character.m_nview.GetZDO().GetBool("Boosted", false)) return;
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
            LoveLamp loveLamp = HaveLoveLampInRange(character.transform.position);
            if(loveLamp)
            {
                loveLamp.Boost(character);
                if(character.m_tameable.IsHungry())
                {
                    loveLamp.DropFood(character.m_tameableMonsterAI.m_consumeItems);
                }
            }
            else UnBoost(character);
        }

        private void DropFood(List<ItemDrop> consumeItems)
        {
            if(!container) return;

            ItemDrop.ItemData foodItem = null;
            Inventory inventory = container.GetInventory();
            foreach(ItemDrop.ItemData item in inventory.GetAllItems())
            {
                if(CanConsume(item, consumeItems))
                {
                    foodItem = item;
                    break;
                }
            }
            if(foodItem == null) return;

            Vector3 position = transform.position + UnityEngine.Random.insideUnitSphere * 1f;
            ItemDrop.DropItem(foodItem, 1, position, Quaternion.identity);
        }

        private bool CanConsume(ItemDrop.ItemData item, List<ItemDrop> consumeItems)
        {
            foreach(ItemDrop consumeItem in consumeItems)
            {
                if(consumeItem.m_itemData.m_shared.m_name == item.m_shared.m_name)
                    return true;
            }
            return false;
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


//CircleProjector