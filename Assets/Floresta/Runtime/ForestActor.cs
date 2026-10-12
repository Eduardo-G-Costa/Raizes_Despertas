using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace Floresta {
 [RequireComponent(typeof(Rigidbody2D))]
 public class ForestActor : MonoBehaviour {
  public float speed=4, battery=100;
  public bool flashlight=true;
  public ForestCampaign campaign;
  Vector2 movement;
  Rigidbody2D body;
  void Awake() {body=GetComponent<Rigidbody2D>();}
  void Update() {
   movement=Vector2.zero;
   if (campaign && campaign.showingMemory) return;
#if ENABLE_INPUT_SYSTEM
   var k=Keyboard.current;
   if(k!=null) {
    movement.x=(k.dKey.isPressed || k.rightArrowKey.isPressed?1:0)-(k.aKey.isPressed || k.leftArrowKey.isPressed?1:0);
    movement.y=(k.wKey.isPressed || k.upArrowKey.isPressed?1:0)-(k.sKey.isPressed || k.downArrowKey.isPressed?1:0);
    if(k.fKey.wasPressedThisFrame) flashlight=!flashlight;
   }
#else
   movement.x=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);
   movement.y=(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0);
   if(Input.GetKeyDown(KeyCode.F)) flashlight=!flashlight;
#endif
   movement=movement.normalized;
   if(flashlight) battery=Mathf.Max(0,battery-Time.deltaTime*0.4f);
   if(battery<=0) flashlight=false;
  }
  void FixedUpdate() {body.MovePosition(body.position+movement*speed*Time.fixedDeltaTime);}
 }
}
