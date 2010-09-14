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
    
    module Home =
        [<Bind("/default/{preferenceReset}"); ReflectedDefinition>]
        let clearPreferenceC (ctx: ictx) preferenceReset (defaultContentType: string) = 
            let updatedContentType = match preferenceReset with | "preferenceReset" -> null | _ -> defaultContentType
            HttpContext.Current.Response.Cookies.Add(new HttpCookie("nrDefaultContentType", defaultContentType))
            updatedContentType |> named "defaultContentType"
        
        [<Bind("/default"); Bind(""); RenderWith("Views/home.django"); ReflectedDefinition>]
        let homeC (ctx: ictx) (preferenceReset: bool option) defaultContentType = 
            
            NDjango.BistroIntegration.DjangoEngine.Provider <- NDjango.BistroIntegration.DjangoEngine.Provider
                                                        .WithFilter "ascontenttype" (new CustomFilters.ContentTypeFilter())

            if not (System.String.IsNullOrEmpty(defaultContentType) || (preferenceReset.IsSome && preferenceReset.Value)) then
                ctx.Transfer(sprintf "/postings/%s" (Content.asString <| Content.fromString defaultContentType))
        
        let invalidNames = new Regex(@"\\|/|\.\.|:", RegexOptions.Compiled)
        [<Bind("/static/{contentId}"); ReflectedDefinition>]
        let staticC (ctx: ictx) contentId = 
            if invalidNames.IsMatch(contentId) then
                raise (WebException(StatusCode.NotFound, sprintf "%A could not be found" contentId))
            else
                ctx.Response.RenderWith(sprintf "static/%s.django" contentId)
                
        [<Bind("?", Priority = 1); ReflectedDefinition>]
        let defaultC (ctx: ictx) = 
            let defaultContentType = 
                match HttpContext.Current.Request.Cookies.["nrDefaultContentType"] with 
                | null -> null | _ as cookie -> cookie.Value
                
            let userType = 
                (if System.String.IsNullOrEmpty(defaultContentType) then ContentType.Resume 
                 else Content.fromString defaultContentType) |>
                asUserType |>
                asString

            defaultContentType, userType
        
                        