namespace WumpusFS.Wumpus
    open System
    open System.Collections.Generic
    open UnityEngine
    open UnityEngine.Events

    module Vec2 = 
        let At a b =
            Vector2.Distance(a, b) < Mathf.Epsilon

    type Percepts =
        struct
            val Stench:bool
            val Breeze:bool
            val Glitter:bool
            new (stench:bool, breeze:bool, glitter:bool) = {Stench = stench; Breeze = breeze; Glitter = glitter}
        end
        
    type Knowledge() as self =
        [<DefaultValue>] val mutable MightHaveWumpus:bool
        [<DefaultValue>] val mutable MightHavePit:bool
        
        member this.Set(wum, pit) =
            self.MightHaveWumpus <- wum
            self.MightHavePit <- pit
        
        static member New(wum, pit) =
            let k = new Knowledge()
            k.MightHaveWumpus <- wum
            k.MightHavePit <- pit
            k
        
        
    type AgentCat() as self = 
        let mutable WorldHeight = -1
        let mutable WorldWidth = -1
        
        let mutable PerceptedPlaces = new Dictionary<Vector2, Percepts>();
        let mutable KnowledgeOfPlaces = new Dictionary<Vector2, Knowledge>();
        
        let mutable Trace = new Stack<Vector2>();
        
        [<DefaultValue>] val mutable FoundGold:bool
        [<DefaultValue>] val mutable CurrentPosition:Vector2
        
       
        member this.TellMeAboutTheWorld(height, width) =
            WorldHeight <- height
            WorldWidth <- width
        
        member this.ClearTrace() =
            Trace.Clear()
            PerceptedPlaces <- new Dictionary<Vector2, Percepts>();
            KnowledgeOfPlaces = new Dictionary<Vector2, Knowledge>();
        
        member this.PercieveCurrentPosition(percepts:Percepts) =
            PerceptedPlaces.[self.CurrentPosition] <- percepts
            KnowledgeOfPlaces.[self.CurrentPosition] <- new Knowledge();
            
            self.FoundGold <- self.FoundGold || percepts.Glitter
            
            let newPlacesToGo = this.PossibleMoves()
            
            for pos in newPlacesToGo do
                if KnowledgeOfPlaces.ContainsKey(pos) then 
                    let knowledge = KnowledgeOfPlaces.[pos]
                    if not percepts.Stench && knowledge.MightHaveWumpus then
                        knowledge.MightHaveWumpus <- false
                    if not percepts.Breeze && knowledge.MightHavePit then 
                        knowledge.MightHavePit <- false
                else
                    KnowledgeOfPlaces.[pos] <- Knowledge.New(percepts.Breeze, percepts.Stench)
        
        member this.WhereIWannaGo():Vector2 =
            if self.FoundGold then
                if  Trace.Count = 0 then
                    Vector2.zero
                else
                    Trace.Pop()
            else
                //Find gold and kill wumpus
                let placesToGo = this.PossibleMoves()
                let placesIveBeen = Seq.where (fun p -> PerceptedPlaces.ContainsKey p) placesToGo
                let newPlacesToGo = Seq.where (fun p -> PerceptedPlaces.ContainsKey p |> not) placesToGo
                let safeNewPlacesToGo = Seq.where IKnowItIsSafe newPlacesToGo
                
                if not (safeNewPlacesToGo |> Seq.isEmpty) then
                    let move = Seq.head safeNewPlacesToGo
                    Trace.Push move
                    move
                else //Dangerous move
                    let dangerousMove = if safeNewPlacesToGo |> Seq.isEmpty |> not then Seq.head newPlacesToGo else Seq.head placesToGo
                    Trace.Push dangerousMove
                    dangerousMove
                    
                    
                    
        let IKnowItIsSafe(pos) =
            KnowledgeOfPlaces.ContainsKey(pos) && 
            not KnowledgeOfPlaces.[pos].MightHaveWumpus && 
            not KnowledgeOfPlaces.[pos].MightHavePit
                    
        
        member this.PossibleMoves() =
            let mutable positions = new List<Vector2>();
            
            if self.CurrentPosition.x > 0.0f then 
                positions.Add(new Vector2((self.CurrentPosition.x - 1.0f), self.CurrentPosition.y))
                
            if self.CurrentPosition.x < float32 WorldHeight - 1.0f then 
                positions.Add(new Vector2((self.CurrentPosition.x + 1.0f), self.CurrentPosition.y))
                
            if self.CurrentPosition.y > 0.0f then 
                positions.Add(new Vector2(self.CurrentPosition.x, self.CurrentPosition.y - float32 1))
                
            if self.CurrentPosition.y < float32 WorldHeight - 1.0f then 
                positions.Add(new Vector2(self.CurrentPosition.x, self.CurrentPosition.y + float32 1))
                
            positions

        
    type CaveWorld() as self =
        
        let mutable Wumpi = List.Empty
        let mutable Pits = List.Empty
        
        let OnMove = new Event<Vector2>()
        let OnWumpusEncountered = new Event<unit>()
        let OnPitEncountered = new Event<unit>()
        let OnTreasureEncountered = new Event<unit>()
        let OnBreezePercepted = new Event<unit>()
        let OnStenchPercepted = new Event<unit>()
        let OnGoalComplete = new Event<unit>()
        
        
        member this.PitAt pos =
            Pits |> Seq.exists (fun pit -> Vec2.At pit pos)
            
        member this.WumpusAt pos =
            Wumpi |> Seq.exists (fun wum -> Vec2.At wum pos)
        
            
        
        member this.WorldHeight = 4
        member this.WorldWidth = 4
        
        [<DefaultValue>] val mutable Gold:Vector2
        [<DefaultValue>] val mutable Cat:AgentCat
        
        member this.GeneratePercepts() =
            let neighbours = this.Cat.PossibleMoves() //Same as the C# implementation with GetNeighbours
            new Percepts(
                  neighbours |> Seq.exists (fun n -> this.PitAt n),
                  neighbours |> Seq.exists (fun n -> this.WumpusAt n),
                  Vec2.At this.Cat.CurrentPosition this.Gold && not this.Cat.FoundGold )
            
        member this.Reset() =
            this.Cat.FoundGold <- false
            this.Cat.ClearTrace()
        
        member this.Initialize(wumpi, pits, gold) =
            Wumpi <- wumpi
            Pits <- pits
            this.Gold <- gold
            this.Cat <- new AgentCat()
            this.Cat.TellMeAboutTheWorld (this.WorldHeight, this.WorldWidth)
        
        member this.Iterate() =
            let agentMove = this.Cat.WhereIWannaGo()
            this.Cat.CurrentPosition <- agentMove
            OnMove.Trigger(agentMove)
            
            if this.Cat.FoundGold && Vec2.At this.Cat.CurrentPosition Vector2.zero then
                OnGoalComplete.Trigger()
                
            if this.WumpusAt this.Cat.CurrentPosition then
                OnWumpusEncountered.Trigger()
            elif this.PitAt this.Cat.CurrentPosition then
                OnPitEncountered.Trigger()
                
            //let percepts = GeneratePer
            
        
                    
            

            
            
            
        
        
        
        
        
        
        
