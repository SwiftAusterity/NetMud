﻿using System.Web.Http;

namespace NetMud
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            #region "AdminAPI"
            config.Routes.MapHttpRoute(
                name: "Api_GetEntityModelView",
                routeTemplate: "api/AdminDataApi/GetEntityModelView/{modelId}",
                defaults: new { controller = "AdminDataApi", action = "GetEntityModelView" }
                );

            config.Routes.MapHttpRoute(
                name: "Api_GetDimensionalData",
                routeTemplate: "api/AdminDataApi/GetDimensionalData/{id}",
                defaults: new { controller = "AdminDataApi", action = "GetDimensionalData" }
                );

            config.Routes.MapHttpRoute(
                name: "Api_RenderRoomEditorWithRadius",
                routeTemplate: "api/AdminDataApi/RenderRoomForEditWithRadius/{id}/{radius}",
                defaults: new { controller = "AdminDataApi", action = "RenderRoomForEditWithRadius" }
                );

            config.Routes.MapHttpRoute(
                name: "Api_RenderZoneMap",
                routeTemplate: "api/AdminDataApi/RenderZoneMap/{id}/{zIndex}",
                defaults: new { controller = "AdminDataApi", action = "RenderZoneMap" }
                );

            config.Routes.MapHttpRoute(
                name: "Api_RenderWorldMap",
                routeTemplate: "api/AdminDataApi/RenderWorldMap/{id}/{zIndex}",
                defaults: new { controller = "AdminDataApi", action = "RenderWorldMap" }
                );
            #endregion

            #region "ClientAPI"
            config.Routes.MapHttpRoute(
              name: "ClientApi_GetEntityModelView",
              routeTemplate: "api/ClientDataApi/GetEntityModelView/{modelId}",
              defaults: new { controller = "ClientDataApi", action = "GetEntityModelView" }
              );

            config.Routes.MapHttpRoute(
                name: "ClientApi_RenderRoomWithRadius",
                routeTemplate: "api/ClientDataApi/RenderRoomWithRadius/{id}/{radius}",
                defaults: new { controller = "ClientDataApi", action = "RenderRoomWithRadius" }
                );

            config.Routes.MapHttpRoute(
               name: "ClientApi_GetUIModule",
               routeTemplate: "api/ClientDataApi/GetUIModuleContent/{moduleName}",
               defaults: new { controller = "ClientDataApi", action = "GetUIModuleContent" }
               );

            config.Routes.MapHttpRoute(
               name: "ClientApi_SaveUIModuleContent",
               routeTemplate: "api/ClientDataApi/SaveUIModuleContent/{moduleName}/{location}",
               defaults: new { controller = "ClientDataApi", action = "SaveUIModuleContent" }
               );

            config.Routes.MapHttpRoute(
               name: "ClientApi_RemoveUIModuleContent",
               routeTemplate: "api/ClientDataApi/RemoveUIModuleContent/{moduleName}/{location}",
               defaults: new { controller = "ClientDataApi", action = "RemoveUIModuleContent" }
               );

            #endregion

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
