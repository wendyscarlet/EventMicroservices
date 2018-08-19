﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebMvc.Models;

namespace WebMvc.Services
{
    public interface IEventCatalogService

    {
       
       Task<EventCatalog> GetEvents(int page, int take, int? category, int? type);

       Task<EventCatalog> GetEventsWithTitle(string title, int page, int take);

       Task<EventCatalog> GetEventsWithTitleCityDate(string title, string city, string date, int page, int take);

       Task<IEnumerable<SelectListItem>> GetEventCategories();

       Task<EventCategoryCatalog> GetEventCategoriesWithImage(int page, int take);

       Task<IEnumerable<SelectListItem>> GetEventTypes();
        
        Task<Event> GetEventItem(int EventId);

        //EventCities
        Task<EventCityCatalog> GetCityInfo(string city);
        Task<EventCatalog> GetEventsInCity(string city);
        Task<IEnumerable<SelectListItem>> GetCities();
       /* Task<EventCityCatalog> GetCityWithId(int? cityFilterApplied,string city,int page,int take);
        Task<EventCatalog> GetEventsWithCityId(int? cityFilterApplied,string city, int page, int take);*/
        
    }
}
