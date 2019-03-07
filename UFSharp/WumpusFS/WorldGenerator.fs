
namespace WumpusFS.WG

    open System
    open System.Collections.Generic
    open UnityEngine
    open UnityEngine
    open UnityEngine
    open UnityEngine
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
        
        let numberOfIterations = 10
        
        let mutable world = new CaveWorld()
        
        [<SerializeField>]
        let mutable PlatformPrefab:GameObject = null
        
        [<SerializeField>]
        let mutable DoorPrefab:GameObject = null
        
        [<SerializeField>]
        let mutable WorldGameObject:GameObject = null
        
        [<SerializeField>]
        let mutable YPosition = 0.0f
        
        
        let mutable WumpusPrefabs = new Dictionary<string, GameObject>()
        let mutable WumpusSounds = new Dictionary<string, AudioClip>()
        
        let mutable iterations = 0
        let mutable comment = ""
        
        let mutable UpdateTimer = 0.0f
        let mutable UpdateTimeSecs = 1.0f
        let mutable iterationNumber = 0
        
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
                    
                    if Vec2.At pos world.Gold then
                        this.Treasure <- (downcast GameObject.Instantiate(WumpusPrefabs.["Treasure"], new Vector3(float32 i, YPosition, float32 j), Quaternion.Euler(0.0f, 180.0f, 0.0f), WorldGameObject.transform) : GameObject)
                    elif world.PitAt pos then
                        GameObject.Instantiate(WumpusPrefabs.["Pit"], new Vector3(float32 i, YPosition, float32 j), Quaternion.Euler(0.0f, 180.0f, 0.0f)) |> ignore
                    elif world.WumpusAt pos then
                        GameObject.Instantiate(WumpusPrefabs.["Wumpus"], new Vector3(float32 i, YPosition, float32 j), Quaternion.Euler(0.0f, 180.0f, 0.0f)) |> ignore
        
        member this.MoveAgent(newPos, moveDuration) =
            let moveDuration = defaultArg moveDuration 1.0f
            this._agent.SetLerpPos(newPos, moveDuration)
            
            
            
            
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
            this.CreateWorldPlatform()
            this._agent <- (downcast GameObject.Instantiate(WumpusPrefabs.["Agent"],
                                                            new Vector3(float32 0, YPosition, float32 0),
                                                            Quaternion.Euler(0.0f, 180.0f, 0.0f))
                                                            : GameObject).GetComponent<Agent>()
            this.EffectsAudioSrc <- this._agent.GetComponent<AudioSource>()
            
            world.OnBreezePercepted.Publish.Add (fun () -> this.PlaySound("Breeze"))
            world.OnMove.Publish.Add (fun pos ->
                let newPos = new Vector3(pos.x, this._agent.transform.position.y, pos.y)
                this._agent.SetLerpPos(newPos)
                this.PlaySound("Move", false))
            world.OnPitEncountered.Publish.Add (fun () ->
                Object.Destroy this._agent
                this.PlaySound("Pit")
                _gameRunning <- false)
            world.OnStenchPercepted.Publish.Add (fun () -> this.PlaySound("Stench"))
            world.OnTreasureEncountered.Publish.Add (fun () ->
                Object.Destroy this.Treasure
                this.PlaySound("Gold"))
            world.OnWumpusEncountered.Publish.Add (fun () ->
                Object.Destroy this._agent
                this.PlaySound("Wumpus")
                _gameRunning <- false)
            world.OnGoalComplete.Publish.Add (fun () ->
                this.PlaySound "Goal"
                world.Reset()
                this.Treasure <- (downcast GameObject.Instantiate(WumpusPrefabs.["Treasure"],
                                                        new Vector3(this.GoldPosition.x, YPosition, this.GoldPosition.y),
                                                        Quaternion.Euler(0.0f, 180.0f, 0.0f))
                                                        : GameObject)
                iterations <- iterations + 1
                _gameRunning <- iterations < numberOfIterations
                comment <- "reset")
                
            
            
        member this.Update() =
            if _gameRunning then
                if UpdateTimer > UpdateTimeSecs then
                    let t = DateTime.UtcNow
                    world.Iterate()
                    UpdateTimer <- 0.0f
                    let runTime = (DateTime.UtcNow.Subtract(t).TotalMilliseconds * 100000.0)
                    //Log LogFile.WriteLine($"{iterationNumber};{t.Elapsed.TotalMilliseconds};{comment}");
                    comment <- ""
                    iterationNumber <- iterationNumber + 1
                else
                    UpdateTimer <- UpdateTimer + Time.deltaTime
            else
                ()  
                
            
                
            
        
        
