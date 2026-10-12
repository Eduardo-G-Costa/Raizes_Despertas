using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Floresta {
 public static class ForestMapBuilder {
  const string Root="Assets/Floresta";
  [MenuItem("Floresta/Criar mapa e cena de teste")]
  public static void Build() {
   if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
   string generated=Root+"/Generated/Mapa_"+System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
   Directory.CreateDirectory(generated);
   AssetDatabase.Refresh();
   string atlasPath=Root+"/Resources/Floresta/tiles.png";
   TextureImporter importer=(TextureImporter)AssetImporter.GetAtPath(atlasPath);
   importer.textureType=TextureImporterType.Default;
   importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
   importer.textureCompression=TextureImporterCompression.Uncompressed;
   importer.npotScale=TextureImporterNPOTScale.None;
   importer.maxTextureSize=2048;importer.SaveAndReimport();
   Texture2D atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
   ForestMapData data=JsonUtility.FromJson<ForestMapData>(File.ReadAllText(Root+"/Resources/Floresta/mapa.json"));
   if(data.tiles.Length!=data.width*data.height) throw new System.Exception("Dimensões inválidas.");
   Tile[] tiles=new Tile[atlas.width/data.tileSize];
   for(int i=0;i<tiles.Length;i++) {
    Sprite sprite=Sprite.Create(atlas,new Rect(i*data.tileSize,0,data.tileSize,data.tileSize),new Vector2(.5f,.5f),data.tileSize,0,SpriteMeshType.FullRect);
    sprite.name="Sprite_"+i;AssetDatabase.CreateAsset(sprite,generated+"/Sprite_"+i+".asset");
    Tile tile=ScriptableObject.CreateInstance<Tile>();tile.sprite=sprite;
    tile.colliderType=Blocked(i)?Tile.ColliderType.Grid:Tile.ColliderType.None;
    AssetDatabase.CreateAsset(tile,generated+"/Tile_"+i+".asset");tiles[i]=tile;
   }
   Scene scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   GameObject root=new GameObject("Floresta_192x160");root.AddComponent<Grid>();
   ForestCampaign campaign=root.AddComponent<ForestCampaign>();
   Tilemap ground=Layer(root,"01_Terreno",0,false);
   Tilemap obstacles=Layer(root,"02_Vegetacao_Colisao",1,true);campaign.vegetation=obstacles;
   TileBase[] floor=new TileBase[data.tiles.Length],wall=new TileBase[data.tiles.Length];
   for(int i=0;i<data.tiles.Length;i++){int t=data.tiles[i];floor[i]=tiles[Blocked(t)?0:t];if(Blocked(t))wall[i]=tiles[t];}
   BoundsInt bounds=new BoundsInt(0,0,0,data.width,data.height,1);
   ground.SetTilesBlock(bounds,floor);obstacles.SetTilesBlock(bounds,wall);
   GameObject points=new GameObject("03_Pontos_de_historia_e_pilhas");points.transform.parent=root.transform;
   foreach(ForestMarker m in data.markers) {
    if(m.kind=="spawn")continue;
    GameObject go=new GameObject(m.id);go.transform.parent=points.transform;go.transform.position=new Vector3(m.x+.5f,m.y+.5f,0);
    if(m.kind=="gate") {
     ForestGate gate=go.AddComponent<ForestGate>();gate.requiredStage=m.stage;
     BoxCollider2D col=go.AddComponent<BoxCollider2D>();col.size=new Vector2(5,5);
     SpriteRenderer sr=go.AddComponent<SpriteRenderer>();sr.sprite=tiles[1].sprite;sr.color=new Color(.55f,.3f,.3f);sr.sortingOrder=4;go.transform.localScale=Vector3.one;
     // Render 5x5 roots separately so transform scale does not rescale collider twice.
     GameObject visual=new GameObject("Raizes");visual.transform.parent=go.transform;visual.transform.localPosition=Vector3.zero;visual.transform.localScale=new Vector3(5,5,1);
     SpriteRenderer vr=visual.AddComponent<SpriteRenderer>();vr.sprite=tiles[1].sprite;vr.color=new Color(.55f,.3f,.3f);vr.sortingOrder=3;
    } else {
     BoxCollider2D col=go.AddComponent<BoxCollider2D>();col.isTrigger=true;col.size=Vector2.one*2;
     ForestPoint p=go.AddComponent<ForestPoint>();p.kind=m.kind;p.stage=m.stage;p.memory=m.text;p.campaign=campaign;
     SpriteRenderer sr=go.AddComponent<SpriteRenderer>();sr.sprite=tiles[m.kind=="book"?8:6].sprite;sr.sortingOrder=5;
     sr.color=m.kind=="battery"?new Color(.7f,1,1):Color.white;go.transform.localScale=m.kind=="battery"?Vector3.one*.55f:Vector3.one;
    }
   }
   ForestMarker spawn=System.Array.Find(data.markers,m=>m.kind=="spawn");
   GameObject player=new GameObject("Jogador_Teste_SUBSTITUIR");player.transform.parent=root.transform;player.transform.position=new Vector3(spawn.x+.5f,spawn.y+.5f,0);
   SpriteRenderer ps=player.AddComponent<SpriteRenderer>();ps.sprite=tiles[8].sprite;ps.color=new Color(.4f,.8f,1);ps.sortingOrder=10;player.transform.localScale=Vector3.one*.75f;
   Rigidbody2D rb=player.AddComponent<Rigidbody2D>();rb.gravityScale=0;rb.freezeRotation=true;rb.interpolation=RigidbodyInterpolation2D.Interpolate;
   CircleCollider2D pc=player.AddComponent<CircleCollider2D>();pc.radius=.4f;
   ForestActor actor=player.AddComponent<ForestActor>();actor.campaign=campaign;campaign.actor=actor;
   GameObject camera=new GameObject("Main Camera");camera.tag="MainCamera";Camera c=camera.AddComponent<Camera>();c.orthographic=true;c.orthographicSize=8;c.backgroundColor=new Color(.03f,.07f,.06f);c.clearFlags=CameraClearFlags.SolidColor;
   ForestCamera follow=camera.AddComponent<ForestCamera>();follow.target=player.transform;follow.actor=actor;follow.width=data.width;follow.height=data.height;camera.transform.position=new Vector3(spawn.x+.5f,spawn.y+.5f,-10);
   camera.AddComponent<AudioListener>();
   AssetDatabase.SaveAssets();
   PrefabUtility.SaveAsPrefabAsset(root,generated+"/Floresta.prefab");
   EditorSceneManager.SaveScene(scene,generated+"/Floresta.unity");
   Selection.activeGameObject=root;
   Debug.Log("Mapa criado em "+generated+". Aperte Play. O menu cria uma cópia nova a cada execução.");
  }
  static bool Blocked(int t){return t==1||t==5||t==6||t==7||t==9||t==10;}
  static Tilemap Layer(GameObject root,string name,int order,bool collision){
   GameObject go=new GameObject(name);go.transform.parent=root.transform;
   Tilemap map=go.AddComponent<Tilemap>();TilemapRenderer renderer=go.AddComponent<TilemapRenderer>();renderer.sortingOrder=order;
   if(collision)go.AddComponent<TilemapCollider2D>();return map;
  }
 }
}
