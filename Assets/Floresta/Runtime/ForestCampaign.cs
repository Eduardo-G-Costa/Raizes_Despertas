using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

namespace Floresta {
 public class ForestCampaign : MonoBehaviour {
  public int memoryStage;
  public bool liberated;
  public UnityEvent<int> onMemory = new UnityEvent<int>();
  public UnityEvent onLiberated = new UnityEvent();
  [TextArea] public string currentMemory;
  public bool showingMemory;
  public Tilemap vegetation;
  public ForestActor actor;
  public void Remember(int stage, string text) {
   if (showingMemory || stage != memoryStage + 1) return;
   memoryStage = stage;
   currentMemory = text;
   showingMemory = true;
   onMemory.Invoke(stage);
   foreach (ForestGate gate in GetComponentsInChildren<ForestGate>(true))
    if (gate.requiredStage <= memoryStage) gate.gameObject.SetActive(false);
  }
  public void BurnBook() {
   if (memoryStage < 5 || liberated) return;
   liberated = true;
   currentMemory = "O grimório se desfaz em cinzas. A floresta silencia. Pai e mãe estão livres.\n\nPonto de integração: flash branco e cutscene do abraço.";
   showingMemory = true;
   if (vegetation) vegetation.color = new Color(1.15f, 1.3f, 1.1f);
   onLiberated.Invoke();
  }
  void OnGUI() {
   GUI.Box(new Rect(12,12,340,64), "Floresta — percurso de teste\nWASD / setas: mover | F: lanterna | E: grimório");
   GUI.Label(new Rect(22,84,350,30), "Memórias: " + memoryStage + "/5" + (actor ? " | Pilha: " + Mathf.CeilToInt(actor.battery) + "%" : ""));
   if (!showingMemory) return;
   float w = Mathf.Min(620, Screen.width-32);
   Rect r = new Rect((Screen.width-w)/2, Screen.height/2-115,w,230);
   GUI.Box(r, "");
   GUIStyle style = new GUIStyle(GUI.skin.label) {wordWrap=true, fontSize=18};
   GUI.Label(new Rect(r.x+20,r.y+20,w-40,140),currentMemory,style);
   if (GUI.Button(new Rect(r.x+w/2-65,r.y+180,130,32), "Continuar")) showingMemory=false;
  }
 }
}
