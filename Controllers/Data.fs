#light

namespace NoRecruiters

module Data =
    open Divan
    open FunctionalDivan.Dsl
    open NoRecruiters.Enums
    open System.Text.RegularExpressions
    open System

    let database = 
        server "localhost" 5984 |>
        db "nr"

    module Entities =
        type tag = {
            tagText: string
            safeText: string
            }

        type posting = {
            id: string; rev: string;
            tags: tag list
            userId: string
            createdOn: System.DateTime
            updatedOn: System.DateTime
            heading: string
            shortname: string
            shorttext: string
            views: int
            deleted: bool
            flagged: bool
            published: bool
            active: bool
            contents: string
            contentType: Enums.Content.ContentType
            }

        type user = {
            id: string
            rev: string
            roles: string list
            userName: string
            password: string
            email: string
            firstName: string
            lastName: string
            postingId: string
            userType: Enums.User.UserType
            }
    
    module Users =
        open Entities
        open System.Security.Cryptography
        open System.Text

        let hashPassword (password: string) =
             let crypto = SHA1.Create()
             let encoding = UnicodeEncoding.UTF8
             encoding.GetString(crypto.ComputeHash(encoding.GetBytes(password)))

        let byName username =
            match selectRecords<Entities.user> (
                                                query "users" "all" database 
                                                |> byKey username
                                                |> limitTo 1
                                                ) with
            | user::[] -> Some user
            | _ -> None

        let byCredentials username password =
            match byName username with
            | Some user ->
                if user.password = hashPassword password then Some user
                else None
            | _ -> None

        let saveUser (user: Entities.user) =
            let id, rev = user |> into database
            { user with id = id; rev = rev }

        // make it be a unit function so that createdOn/updatedOn are set correctly
        let empty() = {
            id = null; rev = null
            roles = []
            userName = System.String.Empty
            password = System.String.Empty
            email = System.String.Empty
            firstName = System.String.Empty
            lastName = System.String.Empty
            postingId = System.String.Empty
            userType = Enums.User.UserType.Company
        }

    module Tags =
        open Entities

        let maxTagLength = 50
        let rankedTags num = 
            selectRecords<Entities.tag> (
                query "items" "alltags" database |> limitTo num
                )

        let rec private doParseAndDedupe tagMap = function
        | h::t -> 
            if Map.containsKey h tagMap then doParseAndDedupe tagMap t
            else doParseAndDedupe (Map.add h (Util.sanitize h) tagMap) t
        | _ -> tagMap
            
        let parseAndDedupe (tags: string) =
            (doParseAndDedupe 
                Map.empty 
                (List.map (fun (elem: string) -> 
                            let trimmed = elem.Trim()
                            trimmed.Substring(0, System.Math.Min(maxTagLength, trimmed.Length)))
                <| (Array.toList (tags.Split([|','|], StringSplitOptions.RemoveEmptyEntries)))) |> 
             Map.toList) |>
            List.map (fun (name, safeName) -> { tagText = name; safeText = safeName })
            
                          
    module Postings =
        open Entities

        let search text (tags: tag list option) (contentType: Enums.Content.ContentType) =
            if String.IsNullOrWhiteSpace(text) && tags.IsNone then
                selectRecords<Entities.posting> (
                    query "items" "byType" database
                    |> byKey contentType
                )
            else
                let t = 
                    match tags with 
                    | Some t when (List.length t) > 0 -> 
                        (System.String.Join(" or tag:", Array.ofList (List.map (fun (e: Entities.tag) -> e.safeText)  t)))
                        |> (if String.IsNullOrWhiteSpace text then sprintf "(tag:%s)"
                            else sprintf "(text:\"%s*\" or title:\"%s*\") and (tag:%s)" text text)
                    | _ -> sprintf "text:\"%s*\" or title:\"%s*\"" text text 
                
                Fti.selectRecords<Entities.posting> (
                    Fti.query "items" "all" database |>
                    Fti.q t
                    )
                
        let byOwner user (published: bool) =
            selectRecords<Entities.posting> (
                query "items" "byOwner" database
                |> byKey [| box user.id; box published |]
                )

        let byId id = 
            id |> from<Entities.posting> database

        let byShortName name =
            match 
                (selectRecords<Entities.posting> (
                    query "items" "byShortName" database |>
                    byKey name
                 )) with
            | h::t -> Some h
            | _ -> None

        let save (posting: Entities.posting) =
            let newId, newRev = posting |> into database
            { posting with id = newId; rev = newRev }

        // make it be a unit function so that createdOn/updatedOn are set correctly
        let empty() = {
            id = null; rev = null
            tags = []
            userId = System.String.Empty
            createdOn = System.DateTime.Now
            updatedOn = System.DateTime.Now
            heading = System.String.Empty
            shortname = System.String.Empty
            shorttext = System.String.Empty
            views = 0
            deleted = false
            flagged = false
            published = false
            active = false
            contents = System.String.Empty
            contentType = Enums.Content.ContentType.Resume
        }

        let htmlPattern = Regex(@"<(.|\n)*?>", RegexOptions.Compiled)
        let makeShortText (contents: string) = 
            let starting = contents.LastIndexOf('>')
            let ending = contents.LastIndexOf('<')
            let s = if ending > starting then contents.Substring(0, ending + 1) else contents
            htmlPattern.Replace(s, System.String.Empty)

        let shortNameLength = 100
        let makeShortName (heading: string) =
            let salt = Random().Next(1000000).ToString()
            let sanitzed = Util.sanitize heading
            sanitzed.[0..System.Math.Min(heading.Length-salt.Length, sanitzed.Length)]