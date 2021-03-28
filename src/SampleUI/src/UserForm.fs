module UserForm

open System
open Feliz
open Fable.React
open Fable.React.Props
open Fable.FormValidation
open Css
open Ctrls

type Model = {
    FName: string
    LName: string
    Email: string
    Phone: string
    BirthDate: DateTime option
}

let init = 
    { FName = "" 
      LName = ""
      Email = ""
      Phone = ""
      BirthDate = None }

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
        if validate() 
        then Toastify.success "Form is valid!"
        else Toastify.error "Please fix validation errors."
        
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
                        Ref (rulesFor fieldName [Required; MinLen 2; MaxLen 50])
                        Class B.``form-control``
                        Value model.FName
                        OnChange (fun e -> setModel { model with FName = e.Value })
                    ]
                )

                let fieldName = "Last Name"
                requiredField fieldName (
                    input [
                        Ref (rulesFor fieldName [Required; MinLen 2; MaxLen 50])
                        Class B.``form-control``
                        Value model.LName
                        OnChange (fun e -> setModel { model with LName = e.Value })
                    ]
                )

                let fieldName = "Email"
                requiredField fieldName (
                    input [
                        Ref (rulesFor fieldName [ 
                            Required 
                            Regex(@"^\S+@\S+$", "Email")
                        ])
                        Class B.``form-control``
                        Value model.Email
                        OnChange (fun e -> setModel { model with Email = e.Value })
                    ]
                )                

                let fieldName = "Phone"
                requiredField fieldName (
                    input [
                        Ref (rulesFor fieldName [ 
                            Required 
                            Regex (@"\(([0-9]{3})\)[-. ]?([0-9]{3})[-. ]?([0-9]{4})", "Phone Number")
                        ])
                        Placeholder "(###) ###-####"
                        Class B.``form-control``
                        Value model.Phone
                        OnChange (fun e -> setModel { model with Phone = e.Value })
                    ]
                )
                
                let fieldName = "Birth Date"
                requiredField fieldName (
                    Html.input [
                        prop.ref (rulesFor fieldName [Required])
                        prop.className "date"
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