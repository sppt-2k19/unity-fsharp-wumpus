namespace WumpusFS
open UnityEngine

type Agent(g:GameObject) as self =
    let mutable go = g
    let mutable transform = g.transform
    let mutable target = Vector3.one
    let mutable spent:float32 = 0.0f
    let mutable dur = 0.0f
    
    member this.Start() =
        Debug.Log ""
       
    member this.Update() =
        spent <- spent + Time.deltaTime
        if Vector3.Distance(transform.position, target) < Mathf.Epsilon then
            transform.position <- target
        else 
            transform.position <- Vector3.Slerp(transform.position, target, spent / dur)
    
    member this.SetLerpPos(pos, ?duration) =
        let duration = defaultArg duration 1.0f
        spent <- 0.0f
        dur <- duration
        target <- pos
        
