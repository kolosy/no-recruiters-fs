#light

namespace NoRecruiters.Filters

open NDjango.Interfaces
open NoRecruiters.Enums.Content

[<Name("ascontenttype")>]
type ContentTypeFilter =
    interface NDjango.Interfaces.ISimpleFilter with
        member x.Perform contentType =
            (match contentType with
                | :? string as str -> if System.String.IsNullOrEmpty str then System.String.Empty else str
                | _ -> asString (contentType :?> ContentType)) :> obj