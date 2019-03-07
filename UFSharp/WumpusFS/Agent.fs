namespace WumpusFS
open UnityEngine

type Agent() as self =
    inherit MonoBehaviour()
    let mutable target = Vector3.one
    let mutable spent:float32 = 0.0f
    let mutable dur = 0.0f
    
    member this.Start() =
        Debug.Log ""
       
    member this.Update() =
        spent <- spent + Time.deltaTime
        if Vector3.Distance(this.transform.position, target) < Mathf.Epsilon then
            this.transform.position <- target
        else 
            this.transform.position <- Vector3.Slerp(this.transform.position, target, spent / dur)
    
    member this.SetLerpPos(pos, ?duration) =
        let duration = defaultArg duration 1.0f
        spent <- 0.0f
        dur <- duration
        target <- pos
        
