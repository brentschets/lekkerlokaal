﻿using LekkerLokaal.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LekkerLokaal.Data.Repositories
{
    public class BestellijnRepository : IBestellijnRepository
    {

        private readonly DbSet<BestelLijn> _bestellijnen;
        private readonly ApplicationDbContext _dbContext;

        public BestellijnRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _bestellijnen = dbContext.BestelLijnen;
        }

        public void Add(BestelLijn bestelLijn)
        {
            _bestellijnen.Add(bestelLijn);
        }

        public IEnumerable<BestelLijn> GetAll()
        {
            return _bestellijnen.AsNoTracking().ToList();
        }

        public BestelLijn GetBy(string qrcode)
        {
            return _bestellijnen.Include(b => b.Bon).SingleOrDefault(g => g.QRCode == qrcode);
        }

        public BestelLijn GetById(int bestellijnid)
        {
            return _bestellijnen.Include(b => b.Bon).Include(b => b.Handelaar).SingleOrDefault(g => g.BestelLijnId == bestellijnid);
        }

        public IEnumerable<BestelLijn> getGebruikteBonnen()
        {
            return _bestellijnen.Where(b => b.Geldigheid == Geldigheid.Gebruikt).Include(b => b.Bon);
        }

        public IEnumerable<BestelLijn> getGebruiktDezeMaand()
        {
            DateTime date = DateTime.Now.Date;
            date = date.AddMonths(-1);
            return _bestellijnen.Where(b => (b.GebruikDatum >= date) && (b.Geldigheid == Geldigheid.Gebruikt));
        }

        public IEnumerable<BestelLijn> getVerkochteBonnen()
        {
            MaakVervallenBonnenVerlopen();
            return _bestellijnen.Where(b => b.Geldigheid != Geldigheid.Ongeldig).Include(b => b.Bon);
        }

        public IEnumerable<BestelLijn> getVerkochtDezeMaand()
        {
            DateTime date = DateTime.Now.Date;
            date = date.AddMonths(-1);
            return _bestellijnen.Where(b => (b.AanmaakDatum >= date) && (b.Geldigheid != Geldigheid.Ongeldig)).Include(b => b.Bon);
        }

        public void MaakVervallenBonnenVerlopen()
        {
            foreach (BestelLijn bon in _bestellijnen.Where(bl => bl.Geldigheid == Geldigheid.Geldig && DateTime.Today > bl.AanmaakDatum.AddYears(1)))
            {
                bon.Geldigheid = Geldigheid.Verlopen;
            }
            SaveChanges();
        }
        
        public void SaveChanges()
        {
            _dbContext.SaveChanges();
        }

        public IEnumerable<BestelLijn> getGebruikteBonnenVanHandelaarId(int id)
        {
            return _bestellijnen.Include(b => b.Handelaar).Where(b => b.Geldigheid == Geldigheid.Gebruikt && b.Handelaar.HandelaarId == id).Include(b => b.Bon).OrderByDescending(b => b.GebruikDatum);
        }
    }
}
