using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace Floresta {
 public class ForestPoint : MonoBehaviour {
  public string kind;
  public int stage;
  [TextArea] public string memory;
  public ForestCampaign campaign;
  bool consumed;
  void OnTriggerEnter2D(Collider2D other) { Visit(other); }
  void OnTriggerStay2D(Collider2D other) { Visit(other); }
  void Visit(Collider2D other) {
   ForestActor actor = other.GetComponentInParent<ForestActor>();
   if (!actor || consumed || campaign.showingMemory) return;
   if (kind=="battery") {actor.battery=Mathf.Min(100,actor.battery+35);consumed=true;gameObject.SetActive(false);}
   if (kind=="memory" && stage==campaign.memoryStage+1) {consumed=true;campaign.Remember(stage,memory);}
   if (kind=="book" && PressE()) campaign.BurnBook();
  }
  static bool PressE() {
#if ENABLE_INPUT_SYSTEM
   return Keyboard.current != null && Keyboard.current.eKey.isPressed;
#else
   return Input.GetKey(KeyCode.E);
#endif
  }
 }
}
