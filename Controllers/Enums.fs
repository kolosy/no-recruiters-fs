#light

namespace NoRecruiters.Enums

open System

module Action = 
    type UserActionType =
    | Recruiter = 1
    | WrongTag = 2
    | Spam = 3
    | SelfDelete = 4

    let asString (action: UserActionType) = Enum.GetName(typeof<UserActionType>,action).ToLower()
    let fromString action = Enum.Parse(typeof<UserActionType>,action,true) :?> UserActionType

module Content = 
    
    type ContentType =
    | Ad = 0
    | Resume = 1
    
    let asString (contentType: ContentType) = Enum.GetName(typeof<ContentType>, contentType).ToLower()
    
    let fromString contentType = Enum.Parse(typeof<ContentType>, contentType, true) :?> ContentType
    
module User =

    type UserType =
    | Company = 0
    | Person = 1
    | Recruiter = 2
    
    let asString (contentType: UserType) = Enum.GetName(typeof<UserType>, contentType).ToLower()

    let fromString (contentType: string) = Enum.Parse (typeof<UserType>, contentType, true) :?> UserType
    
module Common =
    open Content
    open User
    
    let asUserType = function 
    | ContentType.Resume -> UserType.Company
    | ContentType.Ad -> UserType.Person
    | _ -> UserType.Recruiter
    
    let asContentType = function
    | UserType.Company -> ContentType.Resume
    | _ -> ContentType.Ad