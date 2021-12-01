using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Eshop.Data;
using Eshop.Models;

namespace Eshop.Controllers
{
    public class CartsController : Controller
    {
        private readonly EshopContext _context;

        public CartsController(EshopContext context)
        {
            _context = context;
        }

        // GET: Carts
        public async Task<IActionResult> Index()
        {
            string Username = "john";

            var eshopContext = _context.Carts.Include(c => c.Account).Include(c => c.Product)
                               .Where(c => c.Account.Username == Username);
            ViewBag.Total = eshopContext.Sum(c => c.Quantity * c.Product.Price);
            return View(await eshopContext.ToListAsync());
        }

        // GET: Carts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.Account)
                .Include(c => c.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // GET: Carts/Create
        public IActionResult Create()
        {
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Username");
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name");
            return View();
        }

        // POST: Carts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AccountId,ProductId,Quantity")] Cart cart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Username", cart.AccountId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", cart.ProductId);
            return View(cart);
        }

        // GET: Carts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Username", cart.AccountId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", cart.ProductId);
            return View(cart);
        }

        // POST: Carts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AccountId,ProductId,Quantity")] Cart cart)
        {
            if (id != cart.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartExists(cart.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Username", cart.AccountId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", cart.ProductId);
            return View(cart);
        }

        // GET: Carts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.Account)
                .Include(c => c.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // POST: Carts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cart = await _context.Carts.FindAsync(id);
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

       

        //Hiển thị lên View lấy thông tin giỏ hàng jonh
        public IActionResult Pay()
        {

            string username = "john";
            //Lấy tất cả tài khoản jonh
            ViewBag.Account = _context.Accounts.Where(a => a.Username == username).FirstOrDefault();

            //Include để sử dụng Acc.User và Pro.Pri*Quan
            //Tính tổng của giỏ hàng
            ViewBag.CartsTotal = _context.Carts.Include(c => c.Product).Include(c => c.Account)
                                                .Where(c => c.Account.Username == username)
                                                .Sum(c => c.Product.Price * c.Quantity);
            return View();
        }

        //Dùng để người dùng submit
        [HttpPost]
        public IActionResult Pay([Bind("ShippingAddress,ShippingPhone")]Invoice invoice)
        {
            

            string username = "john";

            //Kiểm tra hàng còn hay hết
            if (!CheckStock(username))
            {
                ViewBag.ErrorMessage = "Có sản phẩm đã hết hàng";
                ViewBag.Account = _context.Accounts.Where(a => a.Username == username).FirstOrDefault();
                ViewBag.CartsTotal = _context.Carts.Include(c => c.Product).Include(c => c.Account)
                                              .Where(c => c.Account.Username == username)
                                              .Sum(c => c.Product.Price * c.Quantity);
                return View();
            }    

                //Thêm hóa đơn
                DateTime now = DateTime.Now;
            invoice.Code = now.ToString("yyMMddhhmmss");
            //Lấy id của tài khoản john đầu tiên
            invoice.AccountId = _context.Accounts.FirstOrDefault(a => a.Username == username).Id;
            invoice.IssuedDate = now;
            invoice.Total = _context.Carts.Include(c => c.Product).Include(c => c.Account)
                                                .Where(c => c.Account.Username == username)
                                                .Sum(c => c.Product.Price * c.Quantity);
            _context.Add(invoice);
            _context.SaveChanges();

            //Thêm chi tiết hóa đơn

            //Lấy danh sách hóa đơn
            List<Cart> carts = _context.Carts.Include(c => c.Account).Include(c => c.Product)
                                              .Where(c => c.Account.Username == username).ToList();

            foreach(Cart c in carts)
            {
                InvoiceDetail invoicedetail = new InvoiceDetail();
                invoicedetail.InvoiceId = invoice.Id;
                invoicedetail.ProductId = c.ProductId;
                invoicedetail.Quantity = c.Quantity;
                invoicedetail.UnitPrice = c.Product.Price;
                _context.Add(invoicedetail);
            }
            _context.SaveChanges();

            // Trừ số lượng tồn kho và xóa giỏ hàng
            foreach(Cart c in carts)
            {
                c.Product.Stock -= c.Quantity;
                _context.Carts.Remove(c);
            }
            _context.SaveChanges();
            return RedirectToAction("Index", "Products");
        }

        //Kiểm tra đủ hàng hay không
        private bool CheckStock(string username)
        {
            
            //Lấy giỏ hàng của tài khoản
            List<Cart> carts = _context.Carts.Include(c => c.Account).Include(c => c.Product)
                                             .Where(c => c.Account.Username == username).ToList();
            foreach(Cart c in carts)
            {
                if(c.Product.Stock < c.Quantity)
                {
                    return false;
                }
            }
            return true;
        }
        private bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.Id == id);
        }
    }
}
