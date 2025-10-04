using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers
{
    public class BookController : Controller
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        public IActionResult Index()
        {
            List <Book> books = _bookService.ViewBooks() ?? new();
            return View(books);
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Update(int id)
        {
            var book = _bookService.GetBookById(id);
            if (book == null)
                return NotFound();
            return View(book);
        }

        [HttpPost]
        public IActionResult Create(Book book)
        {
            _bookService.AddBook(book);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Update(int id, Book book)
        {
            if (!ModelState.IsValid)
                return View(book);

            book.Id = id;
            _bookService.UpdateBook(book);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var book = _bookService.GetBookById(id);
            if (book == null)
                return NotFound();

            _bookService.DeleteBookById(id);
            return RedirectToAction("Index");
        }

    }
}
