namespace AdaptiveProxy

open WebSharper

open FSharp.Data.Adaptive

module Server =


    [<Rpc>]
    let computeTime (input: string) =
        async {
            let height  = cval (float input)
            let gravity = cval 9.81
            let timeToFloor = AVal.map2 (fun h g -> sqrt (2.0 * h / g)) height gravity
            return sprintf "%.3fs" (AVal.force timeToFloor )
        }
