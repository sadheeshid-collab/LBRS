using FluentAssertions;
using LBRS.Book.DBContext.Models;
using LBRS.Book.Repo.Interfaces;
using LBRS.Book.Service;
using LBRS.Book.Service.DTOs;
using LBRS.Book.Service.Helper;
using Microsoft.Extensions.Logging;
using Moq;

namespace Book.Service.Tests
{
    public class BookServiceTests
    {
        private readonly Mock<IBookRepository> _bookRepository_Mock;
        private readonly Mock<ILogger<BookService>> _logger;
        private readonly Mock<IHttpClaimContext> _httpClaimContext_Mock;
        private readonly Mock<ILogger<BookService>> logger_Mock;

        private readonly BookService _bookService;
        public BookServiceTests()
        {
            _bookRepository_Mock = new Mock<IBookRepository>();
            _logger = new Mock<ILogger<BookService>>();
            _httpClaimContext_Mock = new Mock<IHttpClaimContext>();

            _httpClaimContext_Mock.Setup(x => x.UserId).Returns(Guid.NewGuid());

            _bookService = new BookService(_bookRepository_Mock.Object,
                _httpClaimContext_Mock.Object,
                _logger.Object);

        }


        #region Book Creation



        [Fact]
        public async Task Book_Add_Should_Return_Success_When_Book_Added()
        {
            var dto = new BookDetailAddDTO
            {
                Title = "Domain-Driven Design",
                ISBN = "978-0321125217",
                Author = "Eric Evans",
                Genre = "Software",
                PublishedYear = 2003,
                Publisher = "Addison-Wesley",
                Description = "Explains how to tackle complex software projects using domain modeling techniques.",
                TotalCopies = 1
            };

            var bookId = Guid.NewGuid();

            _bookRepository_Mock.Setup(x => x.IsIsbnExistsAsync(dto.ISBN))
                .ReturnsAsync(false);

            _bookRepository_Mock.Setup(x => x.Add(It.IsAny<BookDetail>()))
                .ReturnsAsync((true, bookId));




            var responseData = await _bookService.Add(dto);



            responseData.Item1.Should().Be(OperationStatusTypes.Success);
            responseData.Item2.Should().Be(bookId);

            _bookRepository_Mock.Verify(x => x.IsIsbnExistsAsync(dto.ISBN), Times.Once);

            _bookRepository_Mock.Verify(x => x.Add(It.Is<BookDetail>(b =>
                b.Title == dto.Title &&
                b.ISBN == dto.ISBN &&
                b.Author == dto.Author &&
                b.Genre == dto.Genre &&
                b.PublishedYear == dto.PublishedYear &&
                b.Publisher == dto.Publisher &&
                b.Description == dto.Description &&
                b.TotalCopies == dto.TotalCopies &&
                b.AvailableCopies == dto.TotalCopies &&
                b.CreatedByUserID == _httpClaimContext_Mock.Object.UserId
            )), Times.Once);
        }


        [Fact]
        public async Task Book_Add_Should_Return_DuplicateEnrty_When_Book_Added()
        {
            var dto = new BookDetailAddDTO
            {
                Title = "Domain-Driven Design",
                ISBN = "978-0321125217",
                Author = "Eric Evans",
                Genre = "Software",
                PublishedYear = 2003,
                Publisher = "Addison-Wesley",
                Description = "Explains how to tackle complex software projects using domain modeling techniques.",
                TotalCopies = 1
            };

            var bookId = Guid.NewGuid();

            _bookRepository_Mock.Setup(x => x.IsIsbnExistsAsync(dto.ISBN))
                .ReturnsAsync(true);


            var responseData = await _bookService.Add(dto);



            responseData.Item1.Should().Be(OperationStatusTypes.DuplicateEntry);
            responseData.Item2.Should().BeEmpty();

            _bookRepository_Mock.Verify(x => x.IsIsbnExistsAsync(dto.ISBN), Times.Once);
        }


        [Fact]
        public async Task Book_Add_Should_Return_Failure_When_Book_Added()
        {
            var dto = new BookDetailAddDTO
            {
                Title = "Domain-Driven Design",
                ISBN = "978-0321125217",
                Author = "Eric Evans",
                Genre = "Software",
                PublishedYear = 2003,
                Publisher = "Addison-Wesley",
                Description = "Explains how to tackle complex software projects using domain modeling techniques.",
                TotalCopies = 1
            };

            var bookId = Guid.Empty;

            _bookRepository_Mock.Setup(x => x.IsIsbnExistsAsync(dto.ISBN))
                .ReturnsAsync(false);

            _bookRepository_Mock.Setup(x => x.Add(It.IsAny<BookDetail>()))
                .ReturnsAsync((false, bookId));




            var responseData = await _bookService.Add(dto);



            responseData.Item1.Should().Be(OperationStatusTypes.Failure);
            responseData.Item2.Should().Be(bookId);


            _bookRepository_Mock.Verify(x => x.IsIsbnExistsAsync(dto.ISBN), Times.Once);

            _bookRepository_Mock.Verify(x => x.Add(It.Is<BookDetail>(b =>
                b.Title == dto.Title &&
                b.ISBN == dto.ISBN &&
                b.Author == dto.Author &&
                b.Genre == dto.Genre &&
                b.PublishedYear == dto.PublishedYear &&
                b.Publisher == dto.Publisher &&
                b.Description == dto.Description &&
                b.TotalCopies == dto.TotalCopies &&
                b.AvailableCopies == dto.TotalCopies &&
                b.CreatedByUserID == _httpClaimContext_Mock.Object.UserId
            )), Times.Once);
        }

        #endregion



        #region Book Retrieval Logic

        [Fact]
        public async Task Search_Books_Should_Return_Book_Data()
        {
            var inputGenre = "Software";
            var inputAuthor = "Eric";
            var inputPageNumber = 1;
            var inputPageSize = 3; //Requesting 3 records only

            //Book Detail List contains 6 records
            var books = createBookDetails()
                            .Skip(inputPageNumber - 1)
                            .Take(inputPageSize);

            _bookRepository_Mock.Setup(x => x.SearchBooks(inputGenre, inputAuthor, inputPageNumber, inputPageSize))
                .ReturnsAsync(books);



            var responseData = (await _bookService.SearchBooks(inputGenre, inputAuthor, inputPageNumber, inputPageSize)).ToList();


            responseData.Should().HaveCount(inputPageSize);
            responseData[0].IsActive.Should().BeTrue();

            _bookRepository_Mock.Verify(x => x.SearchBooks(inputGenre, inputAuthor, inputPageNumber, inputPageSize), Times.Once);
        }

        [Fact]
        public async Task Search_Books_Should_Return_Empty_Book_Data()
        {
            var inputGenre = "Software";
            var inputAuthor = "Eric";
            var inputPageNumber = 1;
            var inputPageSize = 3; //Requesting 3 records only



            _bookRepository_Mock.Setup(x => x.SearchBooks(inputGenre, inputAuthor, inputPageNumber, inputPageSize))
                .ReturnsAsync(Enumerable.Empty<BookDetail>());



            var responseData = (await _bookService.SearchBooks(inputGenre, inputAuthor, inputPageNumber, inputPageSize)).ToList();


            responseData.Should().BeEmpty();
            //responseData.Should().HaveCount(0);

            _bookRepository_Mock.Verify(x => x.SearchBooks(inputGenre, inputAuthor, inputPageNumber, inputPageSize), Times.Once);
        }

        private List<BookDetail> createBookDetails()
        {
            return new  List<BookDetail>
            {
                new BookDetail
                {
                    BookDetailId = Guid.NewGuid(),
                    Title = "Domain-Driven Design-1",
                    ISBN = "978-0321125211",
                    Author = "Eric Evans",
                    Genre = "Software",
                    PublishedYear = 2003,
                    Publisher = "Addison-Wesley",
                    Description = "Explains how to tackle complex software projects using domain modeling techniques.",
                    TotalCopies = 1,
                    AvailableCopies = 1,
                    IsActive = true
                },
                new BookDetail
                {
                    BookDetailId = Guid.NewGuid(),
                    Title = "Domain-Driven Design-2",
                    ISBN = "978-0321125212",
                    Author = "Eric Evans",
                    Genre = "Software",
                    PublishedYear = 2003,
                    Publisher = "Addison-Wesley",
                    Description = "Explains how to tackle complex software projects using domain modeling techniques.",
                    TotalCopies = 1,
                    AvailableCopies = 1,
                    IsActive = true
                },
                new BookDetail
                {
                    BookDetailId = Guid.NewGuid(),
                    Title = "Domain-Driven Design-3",
                    ISBN = "978-0321125213",
                    Author = "Eric Evans",
                    Genre = "Software",
                    PublishedYear = 2003,
                    Publisher = "Addison-Wesley",
                    Description = "Explains how to tackle complex software projects using domain modeling techniques.",
                    TotalCopies = 1,
                    AvailableCopies = 1,
                    IsActive = true
                },
                new BookDetail
                {
                    BookDetailId = Guid.NewGuid(),
                    Title = "Domain-Driven Design-4",
                    ISBN = "978-0321125214",
                    Author = "Eric Evans",
                    Genre = "Software",
                    PublishedYear = 2003,
                    Publisher = "Addison-Wesley",
                    Description = "Explains how to tackle complex software projects using domain modeling techniques.",
                    TotalCopies = 1,
                    AvailableCopies = 1,
                    IsActive = true
                },
                new BookDetail
                {
                    BookDetailId = Guid.NewGuid(),
                    Title = "Domain-Driven Design-5",
                    ISBN = "978-0321125215",
                    Author = "Eric Evans",
                    Genre = "Software",
                    PublishedYear = 2003,
                    Publisher = "Addison-Wesley",
                    Description = "Explains how to tackle complex software projects using domain modeling techniques.",
                    TotalCopies = 1,
                    AvailableCopies = 1,
                    IsActive = true
                },
                new BookDetail
                {
                    BookDetailId = Guid.NewGuid(),
                    Title = "Domain-Driven Design-6",
                    ISBN = "978-0321125216",
                    Author = "Eric Evans",
                    Genre = "Software",
                    PublishedYear = 2003,
                    Publisher = "Addison-Wesley",
                    Description = "Explains how to tackle complex software projects using domain modeling techniques.",
                    TotalCopies = 1,
                    AvailableCopies = 1,
                    IsActive = true
                }
            };
        }
        #endregion
    }
}