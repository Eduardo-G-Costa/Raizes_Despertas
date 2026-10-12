using UnityEngine;
namespace Floresta {
 public class ForestCamera : MonoBehaviour {
  public Transform target;
  public float width=192,height=160;
  Camera cam;
  Texture2D vignette;
  public ForestActor actor;
  void Awake(){cam=GetComponent<Camera>();}
  void LateUpdate() {
   if(!target)return;
   float hy=cam.orthographicSize,hx=hy*cam.aspect;
   transform.position=new Vector3(Mathf.Clamp(target.position.x,hx,width-hx),Mathf.Clamp(target.position.y,hy,height-hy),-10);
  }
  void OnGUI() {
   if(!actor || (actor.campaign && actor.campaign.liberated))return;
   if(!vignette) {
    vignette=new Texture2D(128,128,TextureFormat.RGBA32,false);
    for(int y=0;y<128;y++)for(int x=0;x<128;x++) {
     float r=new Vector2((x-63.5f)/63.5f,(y-63.5f)/63.5f).magnitude;
     vignette.SetPixel(x,y,new Color(0,0,0,Mathf.SmoothStep(0,.83f,Mathf.Clamp01((r-.35f)/.65f))));
    }
    vignette.Apply();
   }
   Vector3 p=cam.WorldToScreenPoint(actor.transform.position);
   float radius=Screen.height*(actor.flashlight ? .48f : .14f);
   float left=p.x-radius,top=Screen.height-p.y-radius,size=radius*2;
   int old=GUI.depth;GUI.depth=100;
   Color before=GUI.color;GUI.color=new Color(0,0,0,.83f);
   GUI.DrawTexture(new Rect(0,0,Screen.width,Mathf.Max(0,top)),Texture2D.whiteTexture);
   GUI.DrawTexture(new Rect(0,top+size,Screen.width,Mathf.Max(0,Screen.height-top-size)),Texture2D.whiteTexture);
   GUI.DrawTexture(new Rect(0,top,Mathf.Max(0,left),size),Texture2D.whiteTexture);
   GUI.DrawTexture(new Rect(left+size,top,Mathf.Max(0,Screen.width-left-size),size),Texture2D.whiteTexture);
   GUI.color=Color.white;GUI.DrawTexture(new Rect(left,top,size,size),vignette);
   GUI.color=before;GUI.depth=old;
  }
  void OnDestroy(){if(vignette)Destroy(vignette);}
 }
}
