#light

namespace NoRecruiters.Controllers

    open Bistro.FS.Controller
    open Bistro.FS.Definitions
    open Bistro.FS.Inference
    
    open Bistro.Controllers
    open Bistro.Controllers.Descriptor
    open Bistro.Controllers.Descriptor.Data
    open Bistro.Http
    
    open System.Text.RegularExpressions
    open System.Web
    
    open NoRecruiters.Enums
    open NoRecruiters.Enums.Content
    open NoRecruiters.Enums.User
    open NoRecruiters.Enums.Common
    
    open NoRecruiters.Data

    module Aspects =
        [<Bind("get ?/with-tag/{tagList}"); ReflectedDefinition>]
        let tagC tagList currentTags = 
            let tags = match currentTags with | Some t -> t | None -> []
            (if System.String.IsNullOrEmpty(tagList) then tags 
             else (Tags.parseAndDedupe tagList) @ tags)
            |> named "currentTags"

        [<Bind("get ?/without-tag/{tag}"); ReflectedDefinition>]
        let untagC tag (currentTags: Entities.tag list option) = 
            let tags = match currentTags with | Some t -> t | None -> []
            (if System.String.IsNullOrEmpty(tag) then tags 
             else List.filter (fun t -> not (t.safeText = tag)) tags) 
            |> named "currentTags"