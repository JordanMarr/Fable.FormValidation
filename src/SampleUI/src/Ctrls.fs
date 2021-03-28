module Ctrls

open Fable.React
open Fable.React.Props
open Css

let container children = div [Class "container"] children    
let row children = div [Class B.row] children
let col children = div [Class "col"] children
