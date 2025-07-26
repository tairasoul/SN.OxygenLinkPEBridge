using HarmonyLib;
using OxygenLinkPEBridge.Extensions;

namespace OxygenLinkPEBridge.Patches;

struct EventListeners 
{
  public OnAddItem OnAddExpr;
  public OnRemoveItem OnRemoveExpr;
}

[HarmonyPatch(typeof(OxygenLink.OxygenLink))]
class OxygenLinkPatch 
{
  private static List<InventoryItem> GetAllItems(ItemsContainer container) 
  {
    List<InventoryItem> list = [];
    foreach (TechType tech in container.GetItemTypes()) 
    {
      list.AddRange(container.GetItems(tech));
    }
    return list;
  }
  private static Dictionary<ItemsContainer, EventListeners> coll = [];
  private static void OurProcess(OxygenLink.OxygenLink __instance, InventoryItem item, bool remove = false) 
  {
    PickupableStorage? container = item.item.gameObject.GetComponentInChildren<PickupableStorage>();
    if (container != null)
    {
      ItemsContainer iContainer = container.storageContainer.container;
      if (!remove) {
        if (!coll.ContainsKey(iContainer))
        {
          EventListeners listeners = new()
          {
            OnAddExpr = item => OurProcess(__instance, item),
            OnRemoveExpr = item => OurProcess(__instance, item, true)
          };
          coll.Add(iContainer, listeners);
          iContainer.onAddItem += listeners.OnAddExpr;
          iContainer.onRemoveItem += listeners.OnRemoveExpr;
          foreach (InventoryItem item1 in GetAllItems(iContainer)) 
          {
            OurProcess(__instance, item1);
          }
        }
      }
      else 
      {
        if (coll.ContainsKey(iContainer))
        {
          EventListeners listeners = coll.GetValueSafe(iContainer);
          coll.Remove(iContainer);
          iContainer.onAddItem -= listeners.OnAddExpr;
          iContainer.onRemoveItem -= listeners.OnRemoveExpr;
          foreach (InventoryItem item1 in GetAllItems(iContainer)) 
          {
            OurProcess(__instance, item1, true);
          }
        }
      }
    }
    else 
    {
      if (!remove)
        OnAddItem(__instance, item);
      else
        OnRemoveItem(__instance, item);
    }
  }
  
  [HarmonyPatch("ProcessItem")]
  [HarmonyPrefix]
  static bool ProcessItem(OxygenLink.OxygenLink __instance, InventoryItem item, bool remove) 
  {
    OurProcess(__instance, item, remove);
    return false;
  }
  
  static void OnRemoveItem(OxygenLink.OxygenLink instance, InventoryItem item) 
  {
    Oxygen? component = item.item.GetComponent<Oxygen>();
    if (component) 
    {
      instance.GetType().CallMethod("UnlinkOxygenSource", instance, [component]);
    }
  }
  
  static void OnAddItem(OxygenLink.OxygenLink instance, InventoryItem item) 
  {
    Oxygen? component = item.item.GetComponent<Oxygen>();
    if (component) 
    {
      instance.GetType().CallMethod("LinkOxygenSource", instance, [component]);
    }
  }
}