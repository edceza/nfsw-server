﻿using OfflineServer.Servers.Database;
using OfflineServer.Servers.Database.Entities;
using OfflineServer.Servers.Http.Responses;
using System;

namespace OfflineServer.Servers.Http.Classes
{
    public static class Personas
    {
        public static String carslots()
        {
            return Access.CurrentSession.ActivePersona.getCompleteGarage();
        }

        public static String defaultcar()
        {
            String[] splittedPath = Access.sHttp.request.Path.Split('/');
            if (splittedPath.Length == 6)
            {
                Int32 newCarId = Int32.Parse(splittedPath[5]);
                var garage = Access.CurrentSession.ActivePersona.Cars;
                for (int i = 0; i < garage.Count - 1; i++)
                {
                    if (garage[i].Id == newCarId)
                    {
                        Access.CurrentSession.ActivePersona.CurrentCarIndex = i;
                        Access.CurrentSession.ActivePersona.SelectedCar = garage[i];

                        using (var session = SessionManager.getSessionFactory().OpenSession())
                        using (var transaction = session.BeginTransaction())
                        {
                            PersonaEntity personaEntity = session.Load<PersonaEntity>(Access.CurrentSession.ActivePersona.Id);
                            personaEntity.currentCarIndex = i;
                            session.Update(personaEntity);
                            transaction.Commit();
                            break;
                        }
                    }
                }
                return "";
            }
            return Access.CurrentSession.ActivePersona.SelectedCar.getCarEntry().ToString();
        }

        public static String commerce()
        {
            UpdatedCar newCar = Access.sHttp.getPostData().DeserializeObject<CommerceSessionTrans>().updatedCar;
            CommerceSessionResultTrans commerceSessionResultTrans = new CommerceSessionResultTrans();
            UpdatedCar responseCar = new UpdatedCar();
            
            using (var session = SessionManager.getSessionFactory().OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                CarEntity carEntity = session.Load<CarEntity>(Access.CurrentSession.ActivePersona.SelectedCar.Id);
                carEntity.vinyls = newCar.customCar.vinyls;
                carEntity.paints = newCar.customCar.paints;
                carEntity.performanceParts = newCar.customCar.performanceParts;
                carEntity.skillModParts = newCar.customCar.skillModParts;
                carEntity.visualParts = newCar.customCar.visualParts;
                carEntity.heatLevel = 1;

                responseCar.customCar = CustomCar.getCustomCar(carEntity);
                responseCar.durability = carEntity.durability;
                responseCar.heatLevel = 1;
                responseCar.id = carEntity.id;
                responseCar.ownershipType = "CustomizedCar";

                session.Update(carEntity);
                transaction.Commit();
            }

            commerceSessionResultTrans.updatedCar = responseCar;
            return commerceSessionResultTrans.SerializeObject();
        }
    }
}
