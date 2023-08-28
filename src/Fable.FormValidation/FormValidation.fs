module Fable.FormValidation

open Fable.React
open System.Collections.Generic
open Browser.Types
open Fable.Core.JsInterop

let private useState<'T> (initialState: 'T) : ('T * ('T -> unit)) = 
    let state = HookBindings.Hooks.useState<'T>(initialState)
    state.current, state.update

type private RegisteredInputValidators = Dictionary<InputKey, Element * FieldName * Rule list>
and InputKey = string
and FieldName = string
and Rule = 
    | Required
    | MaxLen of int
    | MinLen of int
    | Regex of pattern: string * description: string
    | CustomRule of Result<unit, string>

module RuleFn =
    /// Requires that a given string is not null or white space.
    let required (value: string) =
        if System.String.IsNullOrWhiteSpace(value) then Error "{0} is required" else Ok()

    /// Requires that an obj is not null.
    let requiredObj (value: obj) =
        match value |> Option.ofObj with
        | Some _ -> Ok()
        | None -> Error "{0} is required"

    /// Requires that a string is not shorter than the given min length.
    let minLen (min: int) (value: string) = if (string value).Length < min then Error (sprintf "{0} must be at least %i characters" min) else Ok()

    /// Requires that a string is not longer than the given max length.
    let maxLen (max: int) (value: string) = if (string value).Length > max then Error (sprintf "{0} exceeds the max length of %i" max) else Ok()

    /// Value must be Greater Than n.
    let gt n value = if value > n then Ok() else Error (sprintf "{0} must be greater than %A" n)

    /// Value must be Greater Than or Equal to n.
    let gte n value = if value >= n then Ok() else Error (sprintf "{0} must be greater than %A" n)

    /// Value must be Less Than n.
    let lt n value = if value < n then Ok() else Error (sprintf "{0} must be greater than %A" n)

    /// Value must be Less Than or Equal to n.
    let lte n value = if value <= n then Ok() else Error (sprintf "{0} must be greater than %A" n)

    let regex (pattern: string) (description: string) (value: string) = 
        let m = System.Text.RegularExpressions.Regex.Match(value, pattern)
        if m.Success then Ok() else Error (sprintf "{0} is not a valid %s" description)

/// Validates registered fields, applies or removes markup, and returns a list of error messages.
let private validateAndReturnErrors extract enchance (registeredInputs: RegisteredInputValidators) =
    let registrations = registeredInputs.Values
    [ for (el, fieldName, rules) in registrations do         
        let fieldErrors = 
            rules
            |> List.map (function 
                | Required -> extract el |> RuleFn.required
                | MaxLen max -> extract el |> RuleFn.maxLen max
                | MinLen min -> extract el |> RuleFn.minLen min
                | Regex (pattern, desc) -> extract el |> RuleFn.regex pattern desc
                | CustomRule result -> result
            )
            |> List.choose (function | Error err -> Some err | _ -> None)
            |> List.map (fun err -> System.String.Format(err, fieldName))

        enchance el fieldErrors

        yield! fieldErrors 
    ] |> List.distinct

//// A function that registers a field name and a list of validators with a Ref function.
type RulesFor = FieldName -> Rule list -> Element -> unit
type Validate = unit -> bool
type ResetValidation = unit -> unit
type ValidationErrors = string list

type ValidationArgs =
    { 
        /// A function that extracts a value from a form element.
        GetValue: (Element -> string) option
        /// A function that applies or removes an error highlighting style to a form element.
        SetStyle: (Element -> ValidationErrors -> unit) option 
    }

let setStyleDefault (el: Element) (fieldErrors: ValidationErrors) =
    // Apply or remove error highlighting to fields
    if fieldErrors.Length > 0 then
        el.classList.add("error")
        el.setAttribute("title", fieldErrors.[0])
    else
        el.classList.remove("error")
        el.removeAttribute("title")

let getValueDefault (el: Element): string = el?value

let internal useValidationImpl (args: ValidationArgs) =
        /// Tracks a list of registered elements (by their HashCode or data-vkey) with their validators.
        let registeredInputValidatorsRef = Hooks.useRef(Dictionary<InputKey, Element * FieldName * Rule list>())
        let registeredInputValidators = registeredInputValidatorsRef.current
        let errors, setErrors = useState<string list>([])
        let enabled, setEnabled = useState(false)

        let getValue = defaultArg args.GetValue getValueDefault
        let setStyle = defaultArg args.SetStyle setStyleDefault

        Hooks.useEffect(fun () -> 
            if enabled then
                let errs = validateAndReturnErrors getValue setStyle (registeredInputValidators)
                if errs <> errors then // NOTE: This check prevents "Maximum update depth exceeded" error!!
                    setErrors errs
                    registeredInputValidators.Clear()
        )

        /// Creates a Ref hook for  registering an input for validation (should be set to an input's "Ref" attribute)
        let rulesForRef = Hooks.useRef(fun (fieldName: FieldName) (rules: Rule list) (el: Element) ->
            if el <> null then
                let key =
                    if el.hasAttribute "data-vkey"
                    then el.getAttribute "data-vkey" // This attribute allows user to track an input through toggled visibility
                    else el.GetHashCode().ToString()

                registeredInputValidators.[key] <- (el, fieldName, rules)
        )
        let rulesFor : RulesFor = rulesForRef.current

        /// Enables auto-validation refresh, runs registered validation rules, then returns true if valid.
        let validate() =
            setEnabled true
            let errs = validateAndReturnErrors getValue setStyle (registeredInputValidators)
            setErrors errs
            errs.Length = 0

        /// Disables auto-validation refresh, clears input errors and error summary.
        let resetValidation() = 
            setEnabled false
            registeredInputValidators.Clear()
            setErrors []
            
        (rulesFor: RulesFor), (validate: Validate), (resetValidation: ResetValidation), (errors: ValidationErrors)

/// A hook that provides form input validation.
let useValidation () = 
    useValidationImpl { GetValue = None; SetStyle = None }

open Fable.React.Props

/// Presents a summary list of errors.
let errorSummary (errors: string seq) =
    let errors = errors |> Seq.toList
    if errors.Length > 0 then
        div [Style [Color "darkred"; Border "1px solid red"; Background "#ffe8eb"; Padding "5px"; BorderRadius "5px"; MarginBottom "10px"]] [
            ul [] [ 
                errors
                |> List.mapi (fun idx err -> li [Key (sprintf "err_%i" idx)] [str err] )
                |> ofList
            ]
        ]
    else 
        nothing

type FormValidation = 
    /// <summary>A hook that provides form input validation.</summary>
    /// <param name="getValue">A strategy for extracting the value from form input controls.</param>
    /// <param name="setStyle">A strategy for adding or removing error styles to form input controls.</param>
    /// <returns>A tuple: `rulesFor`, `validate`, `resetValidation`, `validationErrors`</returns>
    static member useValidation(?getValue: Element -> string, ?setStyle: Element -> ValidationErrors -> unit) = 
        useValidationImpl { GetValue = getValue; SetStyle = setStyle }
    