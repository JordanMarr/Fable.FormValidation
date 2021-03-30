# Fable.FormValidation
A Fable React hook library for validating UI inputs and displaying error messages

## Installation
Get it from NuGet!

[![NuGet version (Fable.FormValidation)](https://img.shields.io/nuget/v/Fable.FormValidation.svg?style=flat-square)](https://www.nuget.org/packages/Fable.FormValidation/)


## Sample Form Validation

![validation-animated](https://user-images.githubusercontent.com/1030435/112941069-07166a80-90fc-11eb-9d24-abddaa3cd61e.gif)


## Call the `useValidation()` hook

``` fsharp
open Fable.FormValidation

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


```

## Add validation rules to inputs:

``` fsharp
input [
    Ref (rulesFor "First Name" [Required; MinLen 2; MaxLen 50])
    Class B.``form-control``
    Value model.FName
    OnChange (fun e -> setModel { model with FName = e.Value })
]
```

``` fsharp
input [
    Ref (rulesFor "Email" [ 
        Required 
        Regex(@"^\S+@\S+$", "Email")
    ])
    Class B.``form-control``
    Value model.Email
    OnChange (fun e -> setModel { model with Email = e.Value })
]
```

This example features the Feliz date input with a custom rule:
``` fsharp 
Html.input [
    prop.ref (rulesFor "Birth Date" [
        Required
        CustomRule (
            match model.BirthDate with
            | Some bd -> if bd <= DateTime.Now then Ok() else (Error "Birth Date cannot be a future date")
            | None -> Ok()
        )
    ])
    prop.className "date"
    prop.type'.date
    if model.BirthDate.IsSome
    then prop.value model.BirthDate.Value
    prop.onChange (fun value ->
        let success, bd = DateTime.TryParse value
        if success then setModel { model with BirthDate = Some bd }
    )
]
```

## Add an optional `errorSummary`

``` fsharp
  errorSummary errors
```

## Create an "error" style
When a form input is invalid, the "error" class will be appended. 
You must add styling for invalid inputs in your .css file:
``` css
.error {
	border: 1px solid red;
	background: lightpink;
}
```

## Profit!

![image](https://user-images.githubusercontent.com/1030435/112770388-959fc480-8ff4-11eb-8818-1c446a66c8b5.png)


## Edge Cases
You may encounter a situation where your validated input field is sometimes hidden and then redisplayed (as in the case of a collapsible panel).
This can cause an issue where React regenerates a different hashcode for the input each time it is made visible.  
To resolve this problem, you can add a override "vkey" (validation key) that will be used instead, which will allow Fable.FormValidation to consistently track the input.

``` fsharp
input [
    Ref (rulesFor "First Name" [Required; MinLen 2; MaxLen 50])
    Data ("vkey", ("username-" + model.Id)) // This value must uniquely identify this field
    Class B.``form-control``
    Value model.FName
    OnChange (fun e -> setModel { model with FName = e.Value })
]
```

## Sample App
Click here to see the [full sample](https://github.com/JordanMarr/Fable.FormValidation/blob/main/src/SampleUI/src/UserForm.fs).
