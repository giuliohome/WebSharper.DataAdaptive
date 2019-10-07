namespace AdaptiveProxy

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html
open System
open System.Runtime.InteropServices
open System.Collections
open System.Collections.Generic



[<JavaScript>]
module Extension =

    /// Reference implementation for AdaptiveToken.
    type AdaptiveToken private() =
        static let top = AdaptiveToken()
        static member Top = top

    /// Reference implementations for aval.
    type AdaptiveValue<'T> =
        abstract member GetValue : AdaptiveToken -> 'T

    type aval<'T> = AdaptiveValue<'T>


    [<Proxy(typeof<ChangeableValue<'T>>)>]
    type ChangeableValue<'T>(value : 'T) =
        let mutable value = value

    
        interface AdaptiveValue<'T> with
            member x.GetValue t = value


    and cval<'T> = ChangeableValue<'T>
    

    [<Proxy "FSharp.Data.Adaptive.AValModule, \
      FSharp.Data.Adaptive, Version=0.0.4.0, Culture=neutral, PublicKeyToken=null">]
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module AVal =
            /// Gets the current value for the given adaptive value.
        let force (value : aval<'T>) = 
            value.GetValue(AdaptiveToken.Top)



        let map2 (mapping: 'T1 -> 'T2 -> 'T3) (value1: aval<'T1>) (value2: aval<'T2>) =
            { new aval<'T3> with
                member x.GetValue(t) = mapping (value1.GetValue(t)) (value2.GetValue(t))
            }
                 

open Extension


[<JavaScript>]
module Client =

    let Main () =
        let rvInput = Var.Create ""
        let submit = Submitter.CreateOption rvInput.View
        let vReversed =
            submit.View.MapAsync(function
                | None -> async { return "" }
                | Some input -> Server.computeTime input
            )
        div [] [
            Doc.Input [] rvInput
            Doc.Button "Compute" [] submit.Trigger
            hr [] []
            h4 [attr.``class`` "text-muted"] [text "The server responded:"]
            div [attr.``class`` "jumbotron"] [h1 [] [textView vReversed]]
        ]
        

    let computeTime (input: string) =
        async {
            let height  = cval (float input)
            let gravity = cval 9.81
            let timeToFloor = AVal.map2 (fun h g -> sqrt (2.0 * h / g)) height gravity
            return sprintf "%.3fs" (AVal.force timeToFloor )
        }

        
    let DataProxy () =
        let rvInput = Var.Create ""
        let submit = Submitter.CreateOption rvInput.View
        let vReversed =
            submit.View.MapAsync(function
                | None -> async { return "" }
                | Some input -> computeTime input
            )
        div [] [
            Doc.Input [] rvInput
            Doc.Button "Compute" [] submit.Trigger
            hr [] []
            h4 [attr.``class`` "text-muted"] [text "The Client responded:"]
            div [attr.``class`` "jumbotron"] [h1 [] [textView vReversed]]
        ]
