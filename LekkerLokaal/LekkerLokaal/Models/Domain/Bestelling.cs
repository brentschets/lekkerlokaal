﻿using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;

namespace LekkerLokaal.Models.Domain
{
    public class Bestelling
    {
        public int BestellingId { get; private set; }
        public DateTime BestelDatum { get; set; }
        public ICollection<BestelLijn> BestelLijnen { get; }
        public decimal BestellingTotaal => BestelLijnen.Sum(b => b.Totaal);

        protected Bestelling()
        {
            BestelLijnen = new HashSet<BestelLijn>();
            BestelDatum = DateTime.Today;
        }

        public Bestelling(Winkelwagen winkelwagen) : this()
        {
            if (!winkelwagen.WinkelwagenLijnen.Any())
                throw new InvalidOperationException("Gelieve één of meerdere cadeaubonnen toe te voegen aan uw winkelwagen alvorens u een bestelling plaatst.");

            foreach (WinkelwagenLijn lijn in winkelwagen.WinkelwagenLijnen)
            {
                for (int i = 1; i <= lijn.Aantal; i++)
                {
                    lijn.Bon.AantalBesteld++;
                    string qrcode = String.Format(Guid.NewGuid().ToString() + DateTime.Now.ToString("yyyyMMddhhmmssffffff"));
                    BestelLijnen.Add(new BestelLijn
                    {
                        Bon = lijn.Bon,
                        Aantal = 1,
                        Prijs = lijn.Prijs,
                        Geldigheid = Geldigheid.Ongeldig,
                        AanmaakDatum = DateTime.Today,
                        Handelaar = lijn.Bon.Handelaar,
                        QRCode = qrcode
                    });
                }
            }
        }

        public bool HeeftBesteld(Bon bon) => BestelLijnen.Any(b => b.Bon.Equals(bon));

}
}
