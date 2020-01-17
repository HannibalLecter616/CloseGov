﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProjetoDis.Models;
using ProjetoDis.ProjectClasses.Observer;

namespace ProjetoDis.ProjectClasses.Templates
{
    public abstract class WarningTemplate
    {
        public bool Warning_Template(IDictionary<string, string> requestData)
        {
            if (ValidateData(requestData))
            {
                NotificationData data = CreateWarning(requestData);
                Notify(data);
                return true;
            }
            return false;
        }   

        public abstract bool ValidateData(IDictionary<string, string> request);

        public abstract NotificationData CreateWarning(IDictionary<string, string> request);

        public abstract void Notify(NotificationData data);

    }

    public class AlertTemplate : WarningTemplate
    {
        private static AlertTemplate instance;

        private static readonly object padlock = new object();

        private DateTime date;

        private int danger;

        CloseGovDb db = new CloseGovDb();

        private AlertTemplate()
        {
            //nao se insere nada... construtor vazio
        }

        //validaçao dos dados recebidos no pedido HttpPost
        public override bool ValidateData(IDictionary<string, string> request)
        {
            var check = true;

            foreach (var item in request)
            {
                check = check && String.Compare(item.Value, "") != 0;
            }

            var val_date = DateTime.TryParse(request["date"], out date);

            var num_danger = int.TryParse(request["perigo"], out danger);

            return val_date && check && num_danger ;
        }

        public override NotificationData CreateWarning(IDictionary<string, string> request)
        {
            Alert alert = new Alert();
            alert.Title = request["title"];
            alert.Description = request["description"];
            alert.Location = request["local"];
            alert.Date = date;
            alert.Important = danger;

            db.Alerts.Add(alert);
            db.SaveChanges();

            return new NotificationData(alert.Id, alert.Title, alert.Description, "Alert");
        }

        public override void Notify(NotificationData data)
        {
            Subject subject = new Subject();

            IQueryable<User> query = db.Users.Where(user => user.Type == 2);

            foreach (User user in query)
            {
                UserObserver observer = new UserObserver(user.Id);
                subject.Attach(observer);
            }

            subject.Notify(data);
        }

        //metodo Singleton - evitar a criaçao de multiplas instancias na criaçao de alertas
        public static AlertTemplate Instance
        {
            get 
            {
                //trinco para evitar multiplas instancias em acesso concorrente
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new AlertTemplate();
                    }
                    return instance;
                }
            }
        }
    }

    public class ReportTemplate : WarningTemplate
    {
        private static ReportTemplate instance;

        private static readonly object padlock = new object();

        private DateTime date;

        CloseGovDb db = new CloseGovDb();

        public override bool ValidateData(IDictionary<string, string> request)
        {
            var check = true;

            foreach (var item in request)
            {
                check = check && String.Compare(item.Value, "") != 0;
            }

            var val_date = DateTime.TryParse(request["date"], out date);

            return val_date && check;
        }

        public override NotificationData CreateWarning(IDictionary<string, string> request)
        {
            Report report = new Report();
            report.Title = request["title"];
            report.Description = request["description"];
            report.Location = request["local"];
            report.Date = date;

            db.Reports.Add(report);
            db.SaveChanges();

            return new NotificationData(report.Id, report.Title, report.Description, "Report");
        }

        public override void Notify(NotificationData data)
        {
            Subject subject = new Subject();

            IQueryable<User> query = db.Users.Where(user => user.Type == 1);

            foreach (User user in query)
            {
                UserObserver observer = new UserObserver(user.Id);
                subject.Attach(observer);
            }

            subject.Notify(data);
        }

        public static ReportTemplate Instance
        {
            get
            {
                //trinco para evitar multiplas instancias em acesso concorrente
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new ReportTemplate();
                    }
                    return instance;
                }
            }
        }
    }
}