using CandidateProject.EntityModels;
using CandidateProject.ViewModels;
using log4net;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace CandidateProject.Controllers
{
    [HandleError(ExceptionType = typeof(DbUpdateException), View = "Error")]
    public class CartonController : Controller
    {
        private CartonContext db = new CartonContext();

        private static readonly ILog Log = LogManager.GetLogger(typeof(CartonController));

        // GET: Carton
        public ActionResult Index()
        {
            var cartons = db.Cartons
                .Select(c => new CartonViewModel()
                {
                    Id = c.Id,
                    CartonNumber = c.CartonNumber
                })
                .ToList();

            return View(cartons);
        }

        // GET: Carton/Details/5
        public ActionResult Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var carton = db.Cartons
                    .Where(c => c.Id == id)
                    .Select(c => new CartonViewModel()
                    {
                        Id = c.Id,
                        CartonNumber = c.CartonNumber
                    })
                    .SingleOrDefault();
                if (carton == null)
                {
                    return HttpNotFound();
                }
                return View(carton);
            }
            catch (Exception ex)
            {
                Log.Debug("Exception Details : Method Name - Details(" + id + ") , Controller - Carton", ex);
                Log.Info("Information Details : Method Name - Details(" + id + ") , Controller - Carton", ex);
                Log.Warn("Warning Details : Method Name - Details(" + id + ") , Controller - Carton", ex);

                return View("Error", new HandleErrorInfo(ex, "Carton", "Details"));

            }

        }

        // GET: Carton/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Carton/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CartonNumber")] Carton carton)
        {
            if (ModelState.IsValid)
            {
                db.Cartons.Add(carton);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(carton);
        }

        // GET: Carton/Edit/5
        public ActionResult Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var carton = db.Cartons
                    .Where(c => c.Id == id)
                    .Select(c => new CartonViewModel()
                    {
                        Id = c.Id,
                        CartonNumber = c.CartonNumber
                    })
                    .SingleOrDefault();
                if (carton == null)
                {
                    return HttpNotFound();
                }
                return View(carton);


            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "Carton", "Index"));
            }
        }

        // POST: Carton/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CartonNumber")] CartonViewModel cartonViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var carton = db.Cartons.Find(cartonViewModel.Id);
                    carton.CartonNumber = cartonViewModel.CartonNumber;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(cartonViewModel);
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "Carton", "Index"));
            }

        }

        // GET: Carton/Delete/5
        public ActionResult Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Carton carton = db.Cartons.Find(id);
                if (carton == null)
                {
                    return HttpNotFound();
                }
                return View(carton);
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "Carton", "Index"));
            }

        }

        // POST: Carton/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Carton carton = db.Cartons.Where(i => i.Id == id).FirstOrDefault();
                var Details = db.CartonDetails.Where(j => j.CartonId == carton.Id).AsEnumerable();

                //For Carton Details 
                foreach (var Carton in Details)
                {
                    var details = Carton;
                    db.CartonDetails.Remove(details);
                }
                db.SaveChanges();

                db.Cartons.Remove(carton);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "Carton", "Index"));
            }

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult AddEquipment(int? id)
        {
            try
            {
                var carton = db.Cartons
               .Where(c => c.Id == id)
               .Select(c => new CartonDetailsViewModel()
               {
                   CartonNumber = c.CartonNumber,
                   CartonId = c.Id
               })
               .SingleOrDefault();

                if (carton == null)
                {
                    return HttpNotFound();
                }

                var equipment = db.Equipments
                    .Where(e => !db.CartonDetails.Where(cd => cd.CartonId == id).Select(cd => cd.EquipmentId).Contains(e.Id))
                    .Select(e => new EquipmentViewModel()
                    {
                        Id = e.Id,
                        ModelType = e.ModelType.TypeName,
                        SerialNumber = e.SerialNumber
                    })
                    .ToList();

                carton.Equipment = equipment;
                return View(carton);

            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "Carton", "Index"));
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

        }

        public ActionResult AddEquipmentToCarton([Bind(Include = "CartonId,EquipmentId")] AddEquipmentViewModel addEquipmentViewModel)
        {
            if (ModelState.IsValid)
            {
                var carton = db.Cartons
                    .Include(c => c.CartonDetails)
                    .Where(c => c.Id == addEquipmentViewModel.CartonId)
                    .SingleOrDefault();
                if (carton == null)
                {
                    return HttpNotFound();
                }
                var equipment = db.Equipments
                    .Where(e => e.Id == addEquipmentViewModel.EquipmentId)
                    .SingleOrDefault();
                if (equipment == null)
                {
                    return HttpNotFound();
                }
                var detail = new CartonDetail()
                {
                    Carton = carton,
                    Equipment = equipment
                };
                carton.CartonDetails.Add(detail);
                db.SaveChanges();
            }
            return RedirectToAction("AddEquipment", new { id = addEquipmentViewModel.CartonId });
        }

        public ActionResult ViewCartonEquipment(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var carton = db.Cartons
               .Where(c => c.Id == id)
               .Select(c => new CartonDetailsViewModel()
               {
                   CartonNumber = c.CartonNumber,
                   CartonId = c.Id,
                   Equipment = c.CartonDetails
                       .Select(cd => new EquipmentViewModel()
                       {
                           Id = cd.EquipmentId,
                           ModelType = cd.Equipment.ModelType.TypeName,
                           SerialNumber = cd.Equipment.SerialNumber
                       })
               })
               .SingleOrDefault();
                if (carton == null)
                {
                    return HttpNotFound();
                }
                return View(carton);
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "Carton", "Index"));
            }


        }

        public ActionResult RemoveEquipmentOnCarton([Bind(Include = "CartonId,EquipmentId")] RemoveEquipmentViewModel removeEquipmentViewModel)
        {
            //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            try
            {
                if (ModelState.IsValid)
                {
                    //Remove code here
                    var EquipmentId = db.CartonDetails.Where(d => d.EquipmentId == removeEquipmentViewModel.EquipmentId).First();
                    db.CartonDetails.Remove(EquipmentId);
                    db.SaveChanges();
                }
                return RedirectToAction("ViewCartonEquipment", new { id = removeEquipmentViewModel.CartonId });
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "Carton", "Index"));
            }

        }

        //protected override void OnException(ExceptionContext filterContext)
        //{
        //    Exception exception = filterContext.Exception;
        //    //Logging the Exception
        //    filterContext.ExceptionHandled = true;


        //    var Result = this.View("Error", new HandleErrorInfo(exception,
        //        filterContext.RouteData.Values["controller"].ToString(),
        //        filterContext.RouteData.Values["action"].ToString()));

        //    filterContext.Result = Result;

        //}

    }
}
