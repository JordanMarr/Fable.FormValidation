# Fable.FormValidation
A Fable React hook library for validating UI inputs and displaying error messages

## Installation
Get it from NuGet!

[![NuGet version (Fable.FormValidation)](https://img.shields.io/nuget/v/Fable.FormValidation.svg?style=flat-square)](https://www.nuget.org/packages/Fable.FormValidation/)


## Sample Form

![validation-animated](https://user-images.githubusercontent.com/1030435/112941069-07166a80-90fc-11eb-9d24-abddaa3cd61e.gif)


## Call the useValidation() hook

``` fsharp
open Fable.FormValidation

[<ReactComponent>]
let Page() = 
    let model, setModel = React.useState init
    let rulesFor, validate, resetValidation, errors = useValidation() 

    let save() = 
        if validate() then 
            resetValidation()
            Toastify.success "Form is valid!"
        else 
            Toastify.error "Please fix validation errors."
        
    let cancel() = 
        resetValidation()
        setModel init


```

## Common Validation Rules
* `Rule.Required` -> Validates a textbox text value attribute on validate.
* `Rule.MinLen {n}` -> Validates a textbox text value minimum length on validate.
* `Rule.MaxLen {n}` -> Validates a textbox text value maximum length on validate.
* `Rule.Regex (pattern, desc)` -> Validates a textbox text value with a regext pattern.

**Example:**

``` fsharp
input [
    Ref (rulesFor "First Name" [Required; MaxLen 50])
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

## Custom Validation Rules
* `Rule.CustomRule (fn)` -> Takes any function that returns a `Result<unit,string>`. These rules will directly validate against the current model values and will be calculated during render.

**Example:**

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

## Validating Radio and Checkbox Groups
Validation rules can also be applied to non-input elements!
To validate a radio button group, you can apply validation to the parent container element:

``` fsharp
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
```

## Creating Custom Rule Libraries
It is very easy to extract your custom rules into a reusable library.
*When creating your custom rules, you can templatize the field name with `{0}`:*

``` fsharp
module CustomRules = 
    let mustBeTrue b = CustomRule (if b then Ok() else Error "{0} must be true")

```

## Built-in Rule Functions
You can also use the existing rule functions in the `RuleFn` module in your custom rules.
In fact, some rules, like `gt`, `gte`, `lt` and `lte` exist only as as functions. 
This is because the common rules like `Required` and `MinLen` all expect a textbox text value, so we would lose out of F# type safety if we tried coerce those text values into numeric values at validate time. 
Fortunately, `CustomRule` allows to use these in a type-safe manner:

``` fsharp
input [
    Ref (rulesFor "Amount" [CustomRule (model.Amount |> RuleFn.gte 0)])
    Class B.``form-control``
    Value model.Amount
    OnChange (fun e -> setModel { model with Amount = e.target?value })
]
```

## Add an optional `errorSummary`

``` fsharp
Fable.FormValidation.errorSummary errors
```

## Create an "error" style
When a form input is invalid, the "error" class will be appended. 
You must add styling for invalid inputs in your .css file:
``` css
.error {
    border: 1px solid red;
    background: rgb(255, 232, 235);
}
```

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
