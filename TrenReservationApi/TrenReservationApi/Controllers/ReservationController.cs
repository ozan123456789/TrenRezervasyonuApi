using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace TrainReservationAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReservationController : ControllerBase
    {
        [HttpPost]
        public ActionResult<ReservationResponse> MakeReservation(ReservationRequest request)
        {
            var train = request.Tren;
            var reservationCount = request.RezervasyonYapilacakKisiSayisi;
            var allowDifferentVagons = request.KisilerFarkliVagonlaraYerlestirilebilir;

            var availableVagons = new List<Vagon>();

            foreach (var vagon in train.Vagonlar)
            {
                if (vagon.DoluKoltukAdet + reservationCount <= vagon.Kapasite)
                {
                    availableVagons.Add(vagon);
                }
                else if (allowDifferentVagons && vagon.DoluKoltukAdet < vagon.Kapasite)
                {
                    availableVagons.Add(vagon);
                }
            }

            if (availableVagons.Count == 0)
            {
                return new ReservationResponse
                {
                    RezervasyonYapilabilir = false,
                    YerlesimAyrinti = new List<VagonKisi>()
                };
            }

            var allocationDetails = new List<VagonKisi>();
            var remainingReservationCount = reservationCount;

            foreach (var vagon in availableVagons)
            {
                var availableSeats = vagon.Kapasite - vagon.DoluKoltukAdet;
                var assignedSeats = Math.Min(availableSeats, remainingReservationCount);

                allocationDetails.Add(new VagonKisi
                {
                    VagonAdi = vagon.Ad,
                    KisiSayisi = assignedSeats
                });

                remainingReservationCount -= assignedSeats;

                if (remainingReservationCount == 0)
                {
                    break;
                }
            }

            return new ReservationResponse
            {
                RezervasyonYapilabilir = true,
                YerlesimAyrinti = allocationDetails
            };
        }
    }

    public class ReservationRequest
    {
        public Tren Tren { get; set; }
        public int RezervasyonYapilacakKisiSayisi { get; set; }
        public bool KisilerFarkliVagonlaraYerlestirilebilir { get; set; }
    }

    public class ReservationResponse
    {
        public bool RezervasyonYapilabilir { get; set; }
        public List<VagonKisi> YerlesimAyrinti { get; set; }
    }

    public class Tren
    {
        public string Ad { get; set; }
        public List<Vagon> Vagonlar { get; set; }
    }

    public class Vagon
    {
        public string Ad { get; set; }
        public int Kapasite { get; set; }
        public int DoluKoltukAdet { get; set; }
    }

    public class VagonKisi
    {
        public string VagonAdi { get; set; }
        public int KisiSayisi { get; set; }
    }
}
