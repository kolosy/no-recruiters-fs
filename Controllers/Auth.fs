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
    
    open NoRecruiters.Enums
    open NoRecruiters.Enums.Content
    open NoRecruiters.Enums.User
    open NoRecruiters.Enums.Common
    
    open NoRecruiters.Data
    open NoRecruiters.Util

    module Auth =
        [<Bind("get /auth/signin?{originalRequest}"); 
          RenderWith(@"Views\Profile\signin.django"); ReflectedDefinition>]
        let signInDisplayC (originalRequest: string) = 
            originalRequest

        [<FormData(false)>]
        type signInForm = {
            originalRequest: string
            username: string
            password: string
        }

        [<Bind("post /auth/signin"); 
          RenderWith(@"Views\Profile\signin.django"); ReflectedDefinition>]
        let signInC (ctx: ictx) (data: signInForm) (errors: Map<string, string> option) = 
            let updatedErrors = 
                match normalize data.username, normalize data.password with 
                | "","" -> Some <| reportError errors "username" "Please provide a username and a password"
                | "", _ -> Some <| reportError errors "username" "Please provide a username"
                | _ -> 
                    match Users.byCredentials data.username data.password with
                    | Some user ->
                        ctx.Authenticate <| Security.UserProfile(user)
                        match normalize data.originalRequest with
                        | "" -> ctx.Transfer "/default"
                        | _ -> ctx.Transfer (System.Web.HttpUtility.UrlDecode(data.originalRequest))
                        errors
                    | None -> 
                        Some <| reportError errors "" "The user name or password is incorrect"

            data.username |> named "username",
            updatedErrors |> named "errors"

        [<Bind("get /auth/register");
          RenderWith(@"Views\Profile\register.django"); ReflectedDefinition>]
        let profileDisplayC (ctx: ictx) = ()

        [<FormData(true)>]
        type profileForm = {            
            username: string
            email: string
            firstName: string
            lastName: string
            password: string
        }
        
            

        let validateProfile (profile: profileForm) (errors: Map<string, string> option) =
            let rules = [
                            (System.String.IsNullOrWhiteSpace (profile.username), "username", "Please provide a username");
                            (System.String.IsNullOrWhiteSpace (profile.password), "password", "Please provide a password");
                            ((Users.byName profile.username).IsSome, "username", "A user with the same name already exists. Please choose another name");
                        ]

            validate errors false rules

        [<Bind("post /auth/register");
          RenderWith(@"Views\Profile\register.django"); ReflectedDefinition>]
        let profileC defaultContentType (ctx: ictx) (profile: profileForm) (errors: Map<string, string> option) = 
            match validateProfile profile errors with
            | None ->
                let userType = asUserType <| Content.fromString (if System.String.IsNullOrWhiteSpace defaultContentType then "ad" else defaultContentType)
                ctx.Response.RenderWith(@"Views\Profile\registered.django")
                Users.saveUser 
                    { Users.empty() with
                        userName = profile.username
                        password = Users.hashPassword profile.password
                        userType = userType
                        firstName = profile.firstName
                        lastName = profile.lastName
                        email = profile.email
                        roles = match userType with | UserType.Company -> [ "company" ] | _ -> []
                    } 
                |> Some 
                |> named "currentUser",
                errors
            | _ as newErrors -> None |> named "currentUser", newErrors
