module Toastify

open Fable.Core
open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open System.Text.RegularExpressions

type ToastContainerProps =
    | AutoClose of int

let inline toastContainer (props : ToastContainerProps list) : ReactElement =
    ofImport "ToastContainer" "react-toastify" (keyValueList CaseRules.LowerFirst props) []

let info: url: string -> unit = import "toast.info" "react-toastify"
let success: url: string -> unit = import "toast.success" "react-toastify"
let error: url: string -> unit = import "toast.error" "react-toastify"
let warn: url: string -> unit = import "toast.warn" "react-toastify"
