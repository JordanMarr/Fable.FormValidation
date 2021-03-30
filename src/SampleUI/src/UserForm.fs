module UserForm

open System
open Fable.React
open Fable.React.Props
open Fable.FormValidation
open Css
open Ctrls
open Feliz

type Model = {
    FName: string
    LName: string
    Email: string
    FavoriteLang: DotNetLanguage option
    BirthDate: DateTime option
}
and DotNetLanguage = 
    | FSharp
    | CSharp
    | VB

let init = 
    { FName = "" 
      LName = ""
      Email = ""
      FavoriteLang = None
      BirthDate = Some DateTime.Today }

let requiredField fieldName inputCtrl = 
    div [Class B.``mb-2``] [
        label [Class B.``mr-1``] [str fieldName]
        span [Class "required"; Title "Required"] [str "*"]
        inputCtrl
    ]

[<ReactComponent>]
let Page() = 
    let model, setModel = React.useState init
    let rulesFor, validate, resetValidation, errors = useValidation() 

    let save() = 
        if validate() then 
            resetValidation()
            Toastify.success "Form is valid!"
        
    let cancel() = 
        resetValidation()
        setModel init

    
    container [
        h2 [] [str "User Form"]

        errorSummary errors

        row [
            col [

                let fieldName = "First Name"
                requiredField fieldName (
                    input [
                        Ref (rulesFor fieldName [Required; MaxLen 50])
                        Class B.``form-control``
                        Value model.FName
                        OnChange (fun e -> setModel { model with FName = e.Value })
                    ]
                )

                let fieldName = "Last Name"
                requiredField fieldName (
                    input [
                        Ref (rulesFor fieldName [Required; MaxLen 50])
                        Class B.``form-control``
                        Value model.LName
                        OnChange (fun e -> setModel { model with LName = e.Value })
                    ]
                )

                let fieldName = "Email"
                requiredField fieldName (
                    input [
                        Ref (rulesFor fieldName [Required; Regex(@"^\S+@\S+$", "Email")])
                        Class B.``form-control``
                        Value model.Email
                        OnChange (fun e -> setModel { model with Email = e.Value })
                    ]
                )                

                let fieldName = "Favorite .NET Language"
                let rdoGroup = "FavLangGrp"
                requiredField fieldName (
                    div [
                        Class $"{B.``p-2``} {B.``form-control``}"
                        Style [Width 200]
                        Ref (rulesFor fieldName [
                            CustomRule (
                                match model.FavoriteLang with
                                | None -> Error "{0} is required"
                                | Some lang when lang <> FSharp -> Error "{0} is invalid"
                                | Some lang -> Ok()
                            )
                        ])
                    ] [
                        label [Class B.``mr-4``] [
                            input [
                                Type "radio"
                                Checked (model.FavoriteLang = Some FSharp)
                                Class B.``mr-1``
                                RadioGroup rdoGroup
                                Value "Yes"
                                OnChange (fun e -> setModel { model with FavoriteLang = Some FSharp })
                            ]
                            str "F#"
                        ]

                        label [Class B.``mr-4``] [
                            input [
                                Type "radio"
                                Checked (model.FavoriteLang = Some CSharp)
                                Class B.``mr-1``
                                RadioGroup rdoGroup
                                Value "No"
                                OnChange (fun e -> setModel { model with FavoriteLang = Some CSharp })
                            ]
                            str "C#"
                        ]

                        label [Class B.``mr-4``] [
                            input [
                                Type "radio"
                                Checked (model.FavoriteLang = Some VB)
                                Class B.``mr-1``
                                RadioGroup rdoGroup
                                Value "No"
                                OnChange (fun e -> setModel { model with FavoriteLang = Some VB })
                            ]
                            str "VB"
                        ]
                    ]
                )
                
                let fieldName = "Birth Date"
                requiredField fieldName (
                    Html.input [
                        prop.ref (rulesFor fieldName [
                            Required
                            CustomRule (
                                match model.BirthDate with
                                | Some bd -> if bd <= DateTime.Now then Ok() else (Error "Birth Date cannot be a future date")
                                | None -> Ok()
                            )
                        ])
                        prop.className $"date {B.``form-control``}"
                        prop.style [style.width 200]
                        prop.type'.date
                        if model.BirthDate.IsSome
                        then prop.value model.BirthDate.Value
                        prop.onChange (fun value ->
                            let success, bd = DateTime.TryParse value
                            if success then setModel { model with BirthDate = Some bd }
                        )
                    ]
                )

            ]
        ]

        row [
            col [
                button [Class $"{B.``btn-primary``} {B.``mr-2``}"; OnClick (fun e -> save())] [str "Save"]
                button [Class B.``btn-secondary``; OnClick (fun e -> cancel())] [str "Cancel"]
            ]
        ]
    ]