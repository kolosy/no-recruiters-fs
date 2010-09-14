namespace NoRecruiters
    open Bistro.FS.Inference
    open Bistro.Controllers
    open Bistro.Controllers.Descriptor
    open Bistro.Controllers.Descriptor.Data

    module Util =
        let normalize v =
            if System.String.IsNullOrEmpty v then System.String.Empty
            else v.Trim()

        let safeLower (s: string) =
            if s = null then s
            else s.ToLower()

        let charMap = Map.ofList
                        [
                        ('@', "at");
                        ('#', "sharp");
                        ('$', "dollar");
                        ('%', "percent");
                        ('*', "star");
                        ('+', "plus");
                        (':', "colon");
                        (',', "comma");
                        ('.', "dot");
                        (' ', "_");
                        ('(', System.String.Empty);
                        (')', System.String.Empty);
                        ('{', System.String.Empty);
                        ('}', System.String.Empty);
                        ('|', System.String.Empty);
                        ('[', System.String.Empty);
                        (']', System.String.Empty);
                        ('\\', System.String.Empty);
                        ('"', System.String.Empty);
                        (';', System.String.Empty);
                        ('\'', System.String.Empty);
                        ('<', System.String.Empty);
                        ('>', System.String.Empty);
                        ('?', System.String.Empty);
                        ('/', System.String.Empty);
                        ('`', System.String.Empty);
                        ('~', System.String.Empty);
                        ('!', System.String.Empty);
                        ('^', System.String.Empty);
                        ('&', System.String.Empty);
                        ]

        let sanitize s = 
            String.collect (fun c -> match Map.tryFind c charMap with | Some r -> r | None -> c.ToString()) s

        let reportError (errors: Map<string, string> option) errorCode errorText =
            match errors with 
            | Some e -> 
                if Map.containsKey errorCode e then 
                    let current = e.[errorCode]
                    (Map.remove errorCode e) |>
                    Map.add errorCode (current + ". " + errorText)
                else
                    Map.add errorCode errorText e
            | None ->
                Map.add errorCode errorText Map.empty

        let rec validate errors stopOnFail = function
        | (fail,field,message)::t -> 
            if fail then
                let newErrors = Some (reportError errors field message)
                if stopOnFail then newErrors
                else validate newErrors stopOnFail t
            else
                validate errors stopOnFail t
        | [] -> errors

        [<Bind("?", ControllerBindType = BindType.Payload); ReflectedDefinition>]
        let adjustErrors (errors: Map<string, string> option) =
            (match errors with
             | Some e -> Map.fold (fun (s: System.Collections.Generic.Dictionary<string,string>) k v -> s.Add(k,v); s) (new System.Collections.Generic.Dictionary<string, string>()) e
             | None -> null)
            |> named "errors"
    
 