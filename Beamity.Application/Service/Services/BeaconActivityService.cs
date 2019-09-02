﻿using Beamity.Application.DTOs;
using Beamity.Application.DTOs.BeaconActivityDTOs;
using Beamity.Application.Service.IServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Beamity.Application.DTOs;
using Beamity.Application.Service.IServices;
using Beamity.Core.Models;
using Beamity.EntityFrameworkCore.EntityFrameworkCore.Interfaces;
using Beamity.EntityFrameworkCore.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Beamity.Application.DTOs.CustomDTOs;
using Beamity.Application.DTOs.RoomDTOs;
using Beamity.Application.DTOs.ArtifactDTOs;

namespace Beamity.Application.Service.Services
{
    public class BeaconActivityService : IBeaconActivityService
    {


        private readonly IBaseGenericRepository<BeaconActivity> _beaconActivityRepository;
        private readonly IBaseGenericRepository<Beacon> _beaconRepository;
        private readonly IMapper _mapper;
        public IQueryable<BeaconActivity> publicSet=null;

        public BeaconActivityService(IBaseGenericRepository<BeaconActivity> repository, IBaseGenericRepository<Beacon> beaconRepository, IMapper mapper)
        {
            _beaconActivityRepository = repository;
            _beaconRepository = beaconRepository;
            _mapper = mapper;
            publicSet= _beaconActivityRepository.GetAll();
        }
        public async Task CreateBeaconActivity(CreateBeaconActivityDTO input)
        {
            var beaconActivity = _mapper.Map<BeaconActivity>(input);
            beaconActivity.Beacon = await _beaconRepository.GetById(input.BeaconId);

            await _beaconActivityRepository.Create(beaconActivity);
        }

        public async Task DeleteBeaconActivity(DeleteBeaconActivityDTO input)
        {
            await _beaconActivityRepository.Delete(input.Id);
        }

        public async Task<List<ReadBeaconActivityDTO>> GetAllBeaconActivities(EntityDTO input)
        {
            var beaconActivities = await _beaconActivityRepository
                .GetAll()

                .Include(a=>a.Beacon)
                .ThenInclude(x=>x.Artifact)
                .ThenInclude(x => x.Room)
                .ThenInclude(x => x.Floor)
                .ThenInclude(x => x.Building)
                .ThenInclude(x => x.Location)
                .Where(z => z.IsActive == true).ToListAsync();


            var result = _mapper.Map<List<ReadBeaconActivityDTO>>(beaconActivities);
            /*foreach (var item in beaconActivities)
            {
                ReadBeaconActivityDTO dto = new ReadBeaconActivityDTO();

                dto = _mapper.Map<ReadBeaconActivityDTO>(item);
                dto.BuildingName = item.Building.Name;

                result.Add(dto);
            }*/
            return result;
        }

        public async Task<ReadBeaconActivityDTO> GetBeaconActivity(EntityDTO input)
        {
            var beaconActivity = await _beaconActivityRepository.GetById(input.Id);
            return _mapper.Map<ReadBeaconActivityDTO>(beaconActivity);
        }




        public async Task<double> GetArtifactsVisitorAverage(EntityDTO input)
        {
            //var beaconActivity = await _beaconActivityRepository.GetAll()
            var beaconActivity = await publicSet
                .Include(x => x.Beacon)


                .Where(x => x.EnterTime.Date == DateTime.Now.Date)
                .Where(x => x.Beacon.Artifact.Room.Floor.Building.Location.Id == input.Id)
                 .ToListAsync();

            var subList = from t in beaconActivity
                          group t by t.BeaconId into grouped
                          select new
                          {
                              id = grouped.Key,
                              Count = grouped.Count()

                          };
            double average = subList.Average(x => x.Count);
            average = Math.Round(average, 2);
            return average;
        }


        public async Task<double> GetRoomsVisitorAverage(EntityDTO input)
        {
            //var beaconActivity = await _beaconActivityRepository.GetAll()

           var beaconActivity=await publicSet
                .Include(x=>x.Beacon)
                .ThenInclude(x=>x.Artifact.Room)

                .Where(x => x.EnterTime.Date== DateTime.Now.Date)
                .Where(x=>x.Beacon.Artifact.Room.Floor.Building.Location.Id==input.Id)
                 .ToListAsync();

            var subList = from t in beaconActivity
                          group t by t.Beacon.Artifact.Room.Id into grouped
                          select new
                          {
                              id = grouped.Key,
                              Count = grouped.Count()

                          };
            double average = subList.Average(x => x.Count);
            average = Math.Round(average, 2);
            return average;
        }


        public async Task<double> GetArtifactsWatchTimeAverage(EntityDTO input)
        {
            //var beaconActivity = await _beaconActivityRepository.GetAll()
            var beaconActivity = await publicSet
                .Include(x => x.Beacon)
                
                .Where(x=>x.EnterTime.Date==DateTime.Now.Date)
                .Where(x => x.Beacon.Artifact.Room.Floor.Building.Location.Id == input.Id)
                
                 .ToListAsync();

            var subList = from t in beaconActivity
                          group t by t.Beacon.Id into grouped
                          select new
                          {
                              id = grouped.Key,
                              watchTime=grouped.Sum(t=>(t.ExitTime-t.EnterTime).TotalSeconds)




                          };
            double average = subList.Average(x => x.watchTime);
            average = Math.Round(average, 2);
            return average;
        }


        public async Task<double> GetRoomsWatchTimeAverage(EntityDTO input)
        {
            //var beaconActivity = await _beaconActivityRepository.GetAll()
            var beaconActivity = await publicSet
                .Include(x => x.Beacon)
                .ThenInclude(x=>x.Artifact.Room)

                .Where(x => x.EnterTime.Date == DateTime.Now.Date)
                .Where(x => x.Beacon.Artifact.Room.Floor.Building.Location.Id == input.Id)

                 .ToListAsync();

            var subList = from t in beaconActivity
                          group t by t.Beacon.Artifact.Room.Id into grouped
                          select new
                          {
                              id = grouped.Key,
                              watchTime = grouped.Sum(t => (t.ExitTime - t.EnterTime).TotalSeconds)




                          };
            double average = subList.Average(x => x.watchTime);
            average = Math.Round(average, 2);
            return average;
        }


        public async Task<double> GetLocationBounceRate(EntityDTO input)
        {
            //var beaconActivity = await _beaconActivityRepository.GetAll()
            var beaconActivity = await publicSet
                .Include(x => x.Beacon)
                .ThenInclude(x => x.Artifact.Room)

                //.Where(x=>(x.ExitTime - x.EnterTime).TotalSeconds <=10000)
                .Where(x => x.EnterTime.Date == DateTime.Now.Date)
                .Where(x => x.Beacon.Artifact.Room.Floor.Building.Location.Id == input.Id)

                 .ToListAsync();

            var bounceList = from t in beaconActivity
                          where (t.ExitTime-t.EnterTime).TotalSeconds<=30
                          group t by t.Beacon.Artifact.Id into grouped
                          
                          select new
                          {
                              id = grouped.Key,
                              bounceCount= grouped.Count()




                          };
            double bounceSum = bounceList.Sum(x => x.bounceCount);
            double rate = bounceSum/(beaconActivity.Count)*100;
            rate = Math.Round(rate, 2);
            return rate;
        }


        public async Task<int> GetCurrentVisitors(EntityDTO input)
        {
            //var beaconActivity = await _beaconActivityRepository.GetAll()
            var beaconActivity = await publicSet
                .Include(x => x.Beacon)
                .ThenInclude(x => x.Artifact.Room)

                //.Where(x=>(x.ExitTime - x.EnterTime).TotalSeconds <=10000)
                .Where(x => x.EnterTime.Date == DateTime.Now.Date)
                .Where(x => x.Beacon.Artifact.Room.Floor.Building.Location.Id == input.Id)

                 .ToListAsync();

            var subList = from t in beaconActivity
                             //where (t.ExitTime - t.EnterTime).TotalSeconds <= 100
                             group t by t.UserId into grouped

                             select new
                             {
                                 id = grouped.Key,
                                 latestActivity=grouped.Max(x=>x.ExitTime)




                             };

            var subList2 = from t in subList
                           where (DateTime.Now.TimeOfDay.Subtract(t.latestActivity.TimeOfDay)).TotalMinutes < 30 && (DateTime.Now.TimeOfDay.Subtract(t.latestActivity.TimeOfDay)).TotalMinutes  >= 0 //&& 
                           select new
                           {
                               id = t.id,
                               diff=(DateTime.Now.TimeOfDay.Subtract(t.latestActivity.TimeOfDay))
                           };


            int count= subList2.Count();
            //rate = Math.Round(rate, 2);
            return count;
        }


        public async Task<double> GetUserWatchTimeAverage(EntityDTO input)
        {
            //var beaconActivity = await _beaconActivityRepository.GetAll()
            var beaconActivity = await publicSet
                .Include(x => x.Beacon)
                .ThenInclude(x => x.Artifact.Room)

                .Where(x => x.EnterTime.Date == DateTime.Now.Date)
                .Where(x => x.Beacon.Artifact.Room.Floor.Building.Location.Id == input.Id)

                 .ToListAsync();

            var subList = from t in beaconActivity
                          group t by t.UserId into grouped
                          select new
                          {
                              id = grouped.Key,
                              watchTime = grouped.Sum(t => (t.ExitTime - t.EnterTime).TotalSeconds)




                          };
            double average = subList.Average(x => x.watchTime);
            average = Math.Round(average, 2);
            return average;
        }


        public async Task<double> GetUserArtifactAverage(EntityDTO input)
        {
            //var beaconActivity = await _beaconActivityRepository.GetAll()
            var beaconActivity = await publicSet
                .Include(x => x.Beacon)
                .ThenInclude(x => x.Artifact.Room)

                //.Where(x=>(x.ExitTime - x.EnterTime).TotalSeconds <=10000)
                .Where(x => x.EnterTime.Date == DateTime.Now.Date)
                .Where(x => x.Beacon.Artifact.Room.Floor.Building.Location.Id == input.Id)

                 .ToListAsync();

            var subList = from t in beaconActivity
                              //where (t.ExitTime - t.EnterTime).TotalSeconds <= 100
                          group t by t.UserId into grouped

                          select new
                          {
                              id = grouped.Key,
                              count = grouped.Count()




                          };




            double count = subList.Average(x => x.count);
            //rate = Math.Round(rate, 2);
            return count;
        }

        public async Task<List<MaxMinVisitorArtifactDTO>> GetMaxVisitorArtifact(EntityDTO input)
        {

            DateTime current = DateTime.Now.Date;

            DateTime yesterday = current.AddDays(-1).Date;
            //var beaconActivity = await _beaconActivityRepository.GetAll()
            var beaconActivity = await publicSet
                .Include(x => x.Beacon)
                .ThenInclude(x => x.Artifact.Room)

                //.Where(x=>(x.ExitTime - x.EnterTime).TotalSeconds <=10000)
                .Where(x => x.EnterTime.Date == current || x.EnterTime.Date == yesterday)
                .Where(x => x.Beacon.Artifact.Room.Floor.Building.Location.Id == input.Id)

                 .ToListAsync();

            var todayList = from t in beaconActivity
                          .Where(x => x.EnterTime.Date == current)
                            group t by (t.Beacon.Artifact.Id, t.Beacon.Artifact.Name, t.EnterTime.Date) into grouped

                            select new MaxMinVisitorArtifactDTO
                            {
                                Id1 = grouped.Key.Id,
                                Name = grouped.Key.Name,
                                Count = grouped.Count(),
                                Date = grouped.Key.Date
                          };

            todayList = todayList.OrderByDescending(x => x.Count);

            MaxMinVisitorArtifactDTO todayMax = todayList.First();
            MaxMinVisitorArtifactDTO todayMin = todayList.Last();


            

            var yesterdayList = from t in beaconActivity
                            .Where(x => x.EnterTime.Date == yesterday)
                            .Where(x => x.Beacon.Artifact.Id == todayMax.Id1 || x.Beacon.Artifact.Id == todayMin.Id1)
                                group t by (t.Beacon.Artifact.Id, t.Beacon.Artifact.Name) into grouped

                                select new MaxMinVisitorArtifactDTO
                                {
                                    Id1 = grouped.Key.Id,
                                    Name = grouped.Key.Name,
                                    Count = grouped.Count()
                               };

            yesterdayList = yesterdayList.OrderByDescending(x => x.Count);



            MaxMinVisitorArtifactDTO yesterdayMax = yesterdayList.First();
            MaxMinVisitorArtifactDTO yesterdayMin = yesterdayList.Last();

            List<MaxMinVisitorArtifactDTO> resultSet=new List<MaxMinVisitorArtifactDTO>();
            resultSet.Add(todayMax);
            resultSet.Add(todayMin);
            resultSet.Add(yesterdayMax);
            resultSet.Add(yesterdayMin);

            //rate = Math.Round(rate, 2);
            return resultSet;
        }


        public async Task<List<HourlyVisitorMuseumDTO>> GetHourlyVisitorsMuseum(EntityDTO input)
        {
            //var beaconActivity = await _beaconActivityRepository.GetAll()
            var beaconActivity = await publicSet
                .Include(x => x.Beacon)
                .ThenInclude(x => x.Artifact.Room)

                //.Where(x=>(x.ExitTime - x.EnterTime).TotalSeconds <=10000)
                .Where(x => x.EnterTime.Date == DateTime.Now.Date)
                .Where(x => x.Beacon.Artifact.Room.Floor.Building.Location.Id == input.Id)

                 .ToListAsync();

            var subList = from t in beaconActivity
                              //where (t.ExitTime - t.EnterTime).TotalSeconds <= 100
                          group t by tempClass.TruncateToHourStart(t.EnterTime) into grouped
                          orderby grouped.Key ascending
                          select new HourlyVisitorMuseumDTO
                          {
                              Hour = grouped.Key.TimeOfDay,
                              Count = grouped.Count()




                          };




            var chart = subList.ToList();
            //rate = Math.Round(rate, 2);
            return chart;
        }

        public async Task<List<>> GetHourlyVisitorsArtifact(EntityDTO input)
        {
            //var beaconActivity = await _beaconActivityRepository.GetAll()
            var beaconActivity = await publicSet
                .Include(x => x.Beacon)
                .ThenInclude(x => x.Artifact.Room)

                //.Where(x=>(x.ExitTime - x.EnterTime).TotalSeconds <=10000)
                .Where(x => x.EnterTime.Date == DateTime.Now.Date)
                .Where(x => x.Beacon.Artifact.Room.Floor.Building.Location.Id == input.Id)

                 .ToListAsync();

            var subList = from t in beaconActivity
                              //where (t.ExitTime - t.EnterTime).TotalSeconds <= 100
                          select new ReadRoomDTO
                          {
                              Id=t.Beacon.Artifact.Room.Id,
                              Name=t.Beacon.Artifact.Room.Name
                              
                              




                          };

            var subList2 = from t in beaconActivity
                           select new ReadArtifactDTO
                           {
                               Id = t.Beacon.Artifact.Id,
                               Name = t.Beacon.Artifact.Name,
                               RoomName=t.Beacon.Artifact.Room.Name
                           };


            var subList3 = from t in beaconActivity
                              //where (t.ExitTime - t.EnterTime).TotalSeconds <= 100
                          group t by (tempClass.TruncateToHourStart(t.EnterTime),t.Beacon.Artifact.Id) into grouped
                          orderby grouped.Key ascending
                          select new
                          {
                              Hour = grouped.Key.Item1.TimeOfDay,
                              ArtifactId=grouped.Key.Id,
                              Count = grouped.Count()




                          };
            var list = subList.Distinct().ToList();
            var list2 = subList2.Distinct().ToList();
            var list3 = subList3.ToList();

            var list4 = from k in list2
                        join time in list3 on k.Id equals time.ArtifactId into m
                        select new
                        {
                            Id=k.Id,
                            Name = k.Name,
                            RoomName=k.RoomName,
                            Times=m

                        };
            /* var subList3 = subList.ToList().GroupJoin(subList2.ToList(), room => room.Name, x=>x.RoomName, (room, artifact) => new
             {
                 room.Id,
                 room.Name,
                 artifact.Name

             }
             );*/
            


            var finalList = from d in list
                           join s in list4 on d.Name equals s.RoomName into g
                           
                           select new
                           {
                               RoomName = d.Name,
                               Artifacts = g
                           };
            var final = finalList.ToList();

            //fix by changing join order, time->artifact->room 
            
 

    


            return final;
        }


    }
    static class tempClass
    {
        public static DateTime TruncateToHourStart(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
        }
    }
}

