module App

open Browser.Dom
open Feliz
open Fable.React
open Fable.React.Props
open Css

[<ReactComponent>]
let App() = 
    let routes = {|
        ``/`` = fun _ -> UserForm.Page()
    |}

    let content = 
        HookRouter.useRoutes routes
        |> Option.defaultValue (h1[] [str "Not Found"])

    div [Class B.container] [
        div [Class B.row] [
            div [Class B.col] [
                content
            ]
        ]

        Toastify.toastContainer [Toastify.AutoClose 4000]
    ]