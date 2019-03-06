
namespace WumpusFS.WG

    open System
    open System.Collections.Generic
    open UnityEngine
    open UnityEngine
    open WumpusFS
    open WumpusFS.Wumpus

    type SoundEffect =
        struct
            val Name:string
            val Sound:AudioClip
        end

    type UnityWorldGen() =
        inherit MonoBehaviour()
        
        let mutable world = new CaveWorld()
        
        [<SerializeField>]
        let mutable PlatformPrefab:GameObject = null
        
        [<SerializeField>]
        let mutable DoorPrefab:GameObject = null
        
        [<SerializeField>]
        let mutable WorldGameObject:GameObject = null
        
        [<SerializeField>]
        let mutable YPosition:GameObject = null
        
        
        let mutable WumpusPrefabs = new Dictionary<string, GameObject>()
        let mutable WumpusSounds = new Dictionary<string, AudioClip>()
        
        [<DefaultValue>] val mutable WumpusPositions:List<Vector2>
        [<DefaultValue>] val mutable PitPositions:List<Vector2>
        [<DefaultValue>] val mutable GoldPosition:Vector2
        
        [<DefaultValue>] val mutable _agent:Agent
        [<DefaultValue>] val mutable Treasure:GameObject
        
        [<DefaultValue>] val mutable ObjectPrefabs:List<GameObject>
        [<DefaultValue>] val mutable SoundEffects:List<SoundEffect>
        
        [<DefaultValue>] val mutable MoveAudioSrc:AudioSource
        [<DefaultValue>] val mutable EffectsAudioSrc:AudioSource
        
        
        let mutable _gameRunning = false
        
        //Local methods
        member this.PlaySound(soundName, ?specialEffect0:bool) =
            let specialEffect = defaultArg specialEffect0 false
            let src = if specialEffect then this.EffectsAudioSrc else this.MoveAudioSrc
            src.clip <- WumpusSounds.[soundName]
            src.Play()
        
        
        member this.CreateWorldPlatform() =
            for i in 0 .. world.WorldHeight do
                for j in 0 .. world.WorldWidth do
                    let prefab = if i = 0 && j = 0 then DoorPrefab else PlatformPrefab
                    let platform = GameObject.Instantiate(prefab, new Vector3(float32 i, float32 0, float32 j), Quaternion.identity, WorldGameObject.transform)
                    platform.name <- sprintf "(%d,%d)" i j
                    let pos = new Vector2(float32 i, float32 j)
                    
                    let x = WumpusPrefabs.["Treasure"]
                    if Vec2.At pos world.Gold then
                        this.Treasure <- (downcast GameObject.Instantiate(WumpusPrefabs.["Treasure"], new Vector3(float32 i,float32 YPosition, float32 j), Quaternion.Euler(0.0f, 180.0f, 0.0f), WorldGameObject.transform) : GameObject)
                    //elif world.PitAt 
            
            
            
        //Unity Event methods
        member this.Awake() =
            WumpusPrefabs <- new Dictionary<string, GameObject>()
            WumpusSounds <- new Dictionary<string, AudioClip>()
            
            for p in this.ObjectPrefabs do
                WumpusPrefabs.Add(p.name, p)
            
            for se in this.SoundEffects do
                WumpusSounds.Add(se.Name, se.Sound)
                
            this.MoveAudioSrc = this.GetComponent<AudioSource>()
        
        
        member this.Start() =
            let mode = if Application.isEditor then "editor" else "release"
            world.Initialize(List.ofSeq this.WumpusPositions, List.ofSeq this.PitPositions, this.GoldPosition)
            
        
        member this.Update() =
            Debug.Log YPosition
            
        
        
