IF DB_ID(N'LibraryDB') IS NOT NULL
BEGIN
    ALTER DATABASE LibraryDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE LibraryDB;
END
GO
CREATE DATABASE LibraryDB COLLATE Cyrillic_General_CI_AS;
GO
USE LibraryDB;
GO
CREATE TABLE roles (
    role_id int IDENTITY(1, 1) NOT NULL,
    role_name nvarchar(50) NOT NULL,
    CONSTRAINT pk_roles PRIMARY KEY (role_id),
    CONSTRAINT uq_roles_name UNIQUE (role_name)
);
GO
CREATE TABLE student_groups (
    group_id int IDENTITY(1, 1) NOT NULL,
    group_name nvarchar(30) NOT NULL,
    CONSTRAINT pk_student_groups PRIMARY KEY (group_id),
    CONSTRAINT uq_student_groups_name UNIQUE (group_name)
);
GO
CREATE TABLE loan_statuses (
    status_id int IDENTITY(1, 1) NOT NULL,
    status_name nvarchar(30) NOT NULL,
    CONSTRAINT pk_loan_statuses PRIMARY KEY (status_id),
    CONSTRAINT uq_loan_statuses_name UNIQUE (status_name)
);
GO
CREATE TABLE users (
    user_id int IDENTITY(1, 1) NOT NULL,
    login nvarchar(50) NOT NULL,
    password_hash varchar(64) NOT NULL,
    role_id int NOT NULL,
    last_name nvarchar(50) NOT NULL,
    first_name nvarchar(50) NOT NULL,
    middle_name nvarchar(50) NOT NULL DEFAULT N'-',
    ticket_number nvarchar(20) NOT NULL,
    group_id int NOT NULL,
    phone nvarchar(20) NOT NULL,
    email nvarchar(100) NOT NULL,
    registered_date date NOT NULL DEFAULT (CONVERT(date, GETDATE())),
    is_active bit NOT NULL DEFAULT 1,
    CONSTRAINT pk_users PRIMARY KEY (user_id),
    CONSTRAINT uq_users_login UNIQUE (login),
    CONSTRAINT uq_users_ticket UNIQUE (ticket_number),
    CONSTRAINT fk_users_roles FOREIGN KEY (role_id) REFERENCES roles (role_id),
    CONSTRAINT fk_users_groups FOREIGN KEY (group_id) REFERENCES student_groups (group_id)
);
GO
CREATE TABLE genres (
    genre_id int IDENTITY(1, 1) NOT NULL,
    genre_name nvarchar(100) NOT NULL,
    CONSTRAINT pk_genres PRIMARY KEY (genre_id),
    CONSTRAINT uq_genres_name UNIQUE (genre_name)
);
GO
CREATE TABLE publishers (
    publisher_id int IDENTITY(1, 1) NOT NULL,
    publisher_name nvarchar(100) NOT NULL,
    city nvarchar(50) NOT NULL,
    CONSTRAINT pk_publishers PRIMARY KEY (publisher_id),
    CONSTRAINT uq_publishers_name UNIQUE (publisher_name)
);
GO
CREATE TABLE authors (
    author_id int IDENTITY(1, 1) NOT NULL,
    last_name nvarchar(50) NOT NULL,
    first_name nvarchar(50) NOT NULL,
    middle_name nvarchar(50) NOT NULL DEFAULT N'-',
    CONSTRAINT pk_authors PRIMARY KEY (author_id)
);
GO
CREATE TABLE books (
    book_id int IDENTITY(1, 1) NOT NULL,
    title nvarchar(200) NOT NULL,
    genre_id int NOT NULL,
    publish_year smallint NOT NULL,
    page_count smallint NOT NULL,
    description nvarchar(500) NOT NULL DEFAULT N'Описание отсутствует',
    cover_image nvarchar(100) NOT NULL DEFAULT N'no_cover.jpg',
    CONSTRAINT pk_books PRIMARY KEY (book_id),
    CONSTRAINT ck_books_year CHECK (publish_year BETWEEN 1800 AND 2100),
    CONSTRAINT ck_books_pages CHECK (page_count > 0),
    CONSTRAINT fk_books_genres FOREIGN KEY (genre_id) REFERENCES genres (genre_id)
);
GO
CREATE TABLE book_authors (
    book_author_id int IDENTITY(1, 1) NOT NULL,
    book_id int NOT NULL,
    author_id int NOT NULL,
    CONSTRAINT pk_book_authors PRIMARY KEY (book_author_id),
    CONSTRAINT uq_book_authors UNIQUE (book_id, author_id),
    CONSTRAINT fk_book_authors_books FOREIGN KEY (book_id) REFERENCES books (book_id) ON DELETE CASCADE,
    CONSTRAINT fk_book_authors_authors FOREIGN KEY (author_id) REFERENCES authors (author_id) ON DELETE CASCADE
);
GO
CREATE TABLE book_publishers (
    book_publisher_id int IDENTITY(1, 1) NOT NULL,
    book_id int NOT NULL,
    publisher_id int NOT NULL,
    CONSTRAINT pk_book_publishers PRIMARY KEY (book_publisher_id),
    CONSTRAINT uq_book_publishers UNIQUE (book_id, publisher_id),
    CONSTRAINT fk_book_publishers_books FOREIGN KEY (book_id) REFERENCES books (book_id) ON DELETE CASCADE,
    CONSTRAINT fk_book_publishers_publishers FOREIGN KEY (publisher_id) REFERENCES publishers (publisher_id) ON DELETE CASCADE
);
GO
CREATE TABLE book_copies (
    copy_id int IDENTITY(1, 1) NOT NULL,
    book_id int NOT NULL,
    inventory_number nvarchar(20) NOT NULL,
    date_received date NOT NULL DEFAULT (CONVERT(date, GETDATE())),
    CONSTRAINT pk_book_copies PRIMARY KEY (copy_id),
    CONSTRAINT uq_book_copies_inv UNIQUE (inventory_number),
    CONSTRAINT fk_book_copies_books FOREIGN KEY (book_id) REFERENCES books (book_id)
);
GO
CREATE TABLE loans (
    loan_id int IDENTITY(1, 1) NOT NULL,
    user_id int NOT NULL,
    copy_id int NOT NULL,
    status_id int NOT NULL DEFAULT 1,
    booking_date date NOT NULL DEFAULT (CONVERT(date, GETDATE())),
    due_date date NOT NULL,
    CONSTRAINT pk_loans PRIMARY KEY (loan_id),
    CONSTRAINT ck_loans_dates CHECK (due_date >= booking_date),
    CONSTRAINT fk_loans_users FOREIGN KEY (user_id) REFERENCES users (user_id),
    CONSTRAINT fk_loans_copies FOREIGN KEY (copy_id) REFERENCES book_copies (copy_id),
    CONSTRAINT fk_loans_statuses FOREIGN KEY (status_id) REFERENCES loan_statuses (status_id)
);
GO
CREATE UNIQUE INDEX ux_loans_active_copy ON loans (copy_id) WHERE status_id IN (1, 2, 3, 4);
GO
INSERT INTO roles (role_name) VALUES
    (N'Читатель'),
    (N'Библиотекарь'),
    (N'Администратор');
GO
INSERT INTO student_groups (group_name) VALUES
    (N'Сотрудники'),
    (N'Без группы'),
    (N'320'),
    (N'324');
GO
INSERT INTO loan_statuses (status_name) VALUES
    (N'Ожидает подтверждения'),
    (N'Подтверждена'),
    (N'Выдана'),
    (N'Продлена'),
    (N'Возвращена'),
    (N'Отменена');
GO
INSERT INTO users (login, password_hash, role_id, last_name, first_name, middle_name, ticket_number, group_id, phone, email, registered_date) VALUES
    (N'admin', LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', 'admin123'), 2)), 3, N'Сидоров', N'Алексей', N'Петрович', N'СТ-0001', 1, N'+7 900 100-00-01', N'admin@library.example', '2026-09-01'),
    (N'librarian1', LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', 'lib123'), 2)), 2, N'Фёдорова', N'Татьяна', N'Ивановна', N'СТ-0002', 1, N'+7 900 100-00-02', N'librarian1@library.example', '2026-09-01'),
    (N'librarian2', LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', 'lib456'), 2)), 2, N'Гусева', N'Наталья', N'Сергеевна', N'СТ-0003', 1, N'+7 900 100-00-03', N'librarian2@library.example', '2026-09-01'),
    (N'reader01', LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', 'reader01'), 2)), 1, N'Иванов', N'Пётр', N'Сергеевич', N'ЧБ-0001', 3, N'+7 900 000-00-01', N'reader01@example.com', '2026-09-01'),
    (N'reader02', LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', 'reader02'), 2)), 1, N'Смирнова', N'Анна', N'Андреевна', N'ЧБ-0002', 3, N'+7 900 000-00-02', N'reader02@example.com', '2026-09-01'),
    (N'reader03', LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', 'reader03'), 2)), 1, N'Кузнецов', N'Дмитрий', N'Олегович', N'ЧБ-0003', 3, N'+7 900 000-00-03', N'reader03@example.com', '2026-09-01'),
    (N'reader04', LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', 'reader04'), 2)), 1, N'Попова', N'Мария', N'Игоревна', N'ЧБ-0004', 4, N'+7 900 000-00-04', N'reader04@example.com', '2026-09-01'),
    (N'reader05', LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', 'reader05'), 2)), 1, N'Соколов', N'Артём', N'Викторович', N'ЧБ-0005', 4, N'+7 900 000-00-05', N'reader05@example.com', '2026-09-01'),
    (N'reader06', LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', 'reader06'), 2)), 1, N'Лебедева', N'Елена', N'Павловна', N'ЧБ-0006', 4, N'+7 900 000-00-06', N'reader06@example.com', '2026-09-01'),
    (N'reader07', LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', 'reader07'), 2)), 1, N'Новиков', N'Илья', N'Романович', N'ЧБ-0007', 3, N'+7 900 000-00-07', N'reader07@example.com', '2026-09-01'),
    (N'reader08', LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', 'reader08'), 2)), 1, N'Морозова', N'Полина', N'Денисовна', N'ЧБ-0008', 4, N'+7 900 000-00-08', N'reader08@example.com', '2026-09-01'),
    (N'reader09', LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', 'reader09'), 2)), 1, N'Волков', N'Кирилл', N'Алексеевич', N'ЧБ-0009', 3, N'+7 900 000-00-09', N'reader09@example.com', '2026-09-01'),
    (N'reader10', LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', 'reader10'), 2)), 1, N'Петрова', N'Ирина', N'Николаевна', N'ЧБ-0010', 2, N'+7 900 000-00-10', N'reader10@example.com', '2026-09-01'),
    (N'reader11', LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', 'reader11'), 2)), 1, N'Орлов', N'Сергей', N'Михайлович', N'ЧБ-0011', 2, N'+7 900 000-00-11', N'reader11@example.com', '2026-09-01'),
    (N'reader12', LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', 'reader12'), 2)), 1, N'Захарова', N'Ольга', N'Борисовна', N'ЧБ-0012', 2, N'+7 900 000-00-12', N'reader12@example.com', '2026-09-01');
GO
INSERT INTO genres (genre_name) VALUES
    (N'Информатика и программирование'),
    (N'Математика'),
    (N'Русская классика'),
    (N'Зарубежная классика'),
    (N'Научная фантастика'),
    (N'Физика'),
    (N'История');
GO
INSERT INTO publishers (publisher_name, city) VALUES
    (N'Питер', N'Санкт-Петербург'),
    (N'БХВ-Петербург', N'Санкт-Петербург'),
    (N'Вильямс', N'Москва'),
    (N'Эксмо', N'Москва'),
    (N'АСТ', N'Москва'),
    (N'Лань', N'Санкт-Петербург'),
    (N'Просвещение', N'Москва'),
    (N'ДМК Пресс', N'Москва'),
    (N'Альпина нон-фикшн', N'Москва');
GO
INSERT INTO authors (last_name, first_name, middle_name) VALUES
    (N'Пушкин', N'Александр', N'Сергеевич'),
    (N'Толстой', N'Лев', N'Николаевич'),
    (N'Достоевский', N'Фёдор', N'Михайлович'),
    (N'Булгаков', N'Михаил', N'Афанасьевич'),
    (N'Чехов', N'Антон', N'Павлович'),
    (N'Мартин', N'Роберт', N'-'),
    (N'Кормен', N'Томас', N'-'),
    (N'Лейзерсон', N'Чарльз', N'-'),
    (N'Ривест', N'Рональд', N'-'),
    (N'Штайн', N'Клиффорд', N'-'),
    (N'Азимов', N'Айзек', N'-'),
    (N'Брэдбери', N'Рэй', N'-'),
    (N'Оруэлл', N'Джордж', N'-'),
    (N'Скит', N'Джон', N'-'),
    (N'Фейнман', N'Ричард', N'-'),
    (N'Ключевский', N'Василий', N'Осипович'),
    (N'Фихтенгольц', N'Григорий', N'Михайлович');
GO
INSERT INTO books (title, genre_id, publish_year, page_count, description, cover_image) VALUES
    (N'Евгений Онегин', 3, 2020, 288, N'Роман в стихах о жизни русского дворянства начала XIX века.', N'book_01.jpg'),
    (N'Война и мир', 3, 2019, 1296, N'Роман-эпопея о русском обществе в эпоху наполеоновских войн.', N'book_02.jpg'),
    (N'Преступление и наказание', 3, 2021, 608, N'Психологический роман о преступлении и нравственном выборе.', N'book_03.jpg'),
    (N'Мастер и Маргарита', 3, 2020, 480, N'Роман о визите дьявола в Москву 1930-х годов.', N'book_04.jpg'),
    (N'Вишнёвый сад', 3, 2018, 96, N'Пьеса о судьбе дворянского имения.', N'book_05.jpg'),
    (N'Чистый код', 1, 2022, 464, N'Приёмы написания читаемого и поддерживаемого программного кода.', N'book_06.jpg'),
    (N'Алгоритмы: построение и анализ', 1, 2022, 1328, N'Фундаментальный учебник по алгоритмам и структурам данных.', N'book_07.jpg'),
    (N'C# в глубину', 1, 2020, 528, N'Углублённое описание возможностей языка C# и платформы .NET.', N'book_08.jpg'),
    (N'Чистая архитектура', 1, 2021, 352, N'Принципы проектирования программных систем.', N'book_09.jpg'),
    (N'Курс дифференциального и интегрального исчисления', 2, 2022, 608, N'Классический учебник математического анализа.', N'book_10.jpg'),
    (N'Курс русской истории', 7, 2021, 736, N'Лекции по истории России с древнейших времён.', N'book_11.jpg'),
    (N'Я, робот', 5, 2020, 320, N'Сборник рассказов о роботах и законах робототехники.', N'book_12.jpg'),
    (N'Основание', 5, 2021, 320, N'Роман о падении галактической империи.', N'book_13.jpg'),
    (N'451 градус по Фаренгейту', 5, 2020, 256, N'Роман-антиутопия о мире, где сжигают книги.', N'book_14.jpg'),
    (N'1984', 4, 2021, 320, N'Роман-антиутопия о тоталитарном обществе.', N'book_15.jpg'),
    (N'Скотный двор', 4, 2020, 128, N'Притча-сатира о судьбе революции.', N'book_16.jpg'),
    (N'Фейнмановские лекции по физике. Том 1', 6, 2021, 440, N'Вводный курс физики: механика, излучение, теплота.', N'book_17.jpg'),
    (N'Рассказы', 3, 2019, 352, N'Избранные рассказы А. П. Чехова.', N'book_18.jpg'),
    (N'Братья Карамазовы', 3, 2020, 832, N'Философский роман о вере, свободе и ответственности.', N'book_19.jpg'),
    (N'Идиот', 3, 2020, 640, N'Роман о князе Мышкине и столкновении идеала с обществом.', N'book_20.jpg');
GO
INSERT INTO book_authors (book_id, author_id) VALUES
    (1, 1),
    (2, 2),
    (3, 3),
    (4, 4),
    (5, 5),
    (6, 6),
    (7, 7),
    (7, 8),
    (7, 9),
    (7, 10),
    (8, 14),
    (9, 6),
    (10, 17),
    (11, 16),
    (12, 11),
    (13, 11),
    (14, 12),
    (15, 13),
    (16, 13),
    (17, 15),
    (18, 5),
    (19, 3),
    (20, 3);
GO
INSERT INTO book_publishers (book_id, publisher_id) VALUES
    (1, 5),
    (2, 4),
    (3, 4),
    (4, 5),
    (5, 7),
    (6, 1),
    (7, 3),
    (8, 8),
    (9, 1),
    (10, 6),
    (11, 4),
    (12, 4),
    (13, 4),
    (14, 4),
    (15, 5),
    (16, 5),
    (17, 9),
    (18, 7),
    (19, 4),
    (20, 4);
GO
INSERT INTO book_copies (book_id, inventory_number, date_received) VALUES
    (1, N'И-0001', '2024-09-01'),
    (1, N'И-0002', '2024-09-01'),
    (1, N'И-0003', '2024-09-01'),
    (2, N'И-0004', '2024-10-01'),
    (2, N'И-0005', '2024-10-01'),
    (2, N'И-0006', '2024-10-01'),
    (3, N'И-0007', '2024-10-31'),
    (3, N'И-0008', '2024-10-31'),
    (4, N'И-0009', '2024-11-30'),
    (4, N'И-0010', '2024-11-30'),
    (4, N'И-0011', '2024-11-30'),
    (5, N'И-0012', '2024-12-30'),
    (5, N'И-0013', '2024-12-30'),
    (6, N'И-0014', '2025-01-29'),
    (6, N'И-0015', '2025-01-29'),
    (6, N'И-0016', '2025-01-29'),
    (7, N'И-0017', '2024-09-01'),
    (7, N'И-0018', '2024-09-01'),
    (8, N'И-0019', '2024-10-01'),
    (8, N'И-0020', '2024-10-01'),
    (9, N'И-0021', '2024-10-31'),
    (9, N'И-0022', '2024-10-31'),
    (10, N'И-0023', '2024-11-30'),
    (10, N'И-0024', '2024-11-30'),
    (11, N'И-0025', '2024-12-30'),
    (11, N'И-0026', '2024-12-30'),
    (12, N'И-0027', '2025-01-29'),
    (12, N'И-0028', '2025-01-29'),
    (13, N'И-0029', '2024-09-01'),
    (13, N'И-0030', '2024-09-01'),
    (14, N'И-0031', '2024-10-01'),
    (14, N'И-0032', '2024-10-01'),
    (15, N'И-0033', '2024-10-31'),
    (15, N'И-0034', '2024-10-31'),
    (15, N'И-0035', '2024-10-31'),
    (16, N'И-0036', '2024-11-30'),
    (16, N'И-0037', '2024-11-30'),
    (17, N'И-0038', '2024-12-30'),
    (17, N'И-0039', '2024-12-30'),
    (18, N'И-0040', '2025-01-29'),
    (18, N'И-0041', '2025-01-29'),
    (19, N'И-0042', '2024-09-01'),
    (19, N'И-0043', '2024-09-01'),
    (20, N'И-0044', '2024-10-01'),
    (20, N'И-0045', '2024-10-01');
GO
INSERT INTO loans (user_id, copy_id, status_id, booking_date, due_date) VALUES
    (4, 14, 5, '2026-09-01', '2026-09-15'),
    (5, 1, 5, '2026-09-02', '2026-09-16'),
    (6, 17, 5, '2026-09-03', '2026-09-24'),
    (7, 27, 5, '2026-09-04', '2026-09-18'),
    (8, 9, 5, '2026-09-07', '2026-09-21'),
    (9, 33, 5, '2026-09-08', '2026-09-22'),
    (13, 25, 5, '2026-09-09', '2026-09-23'),
    (10, 4, 5, '2026-09-10', '2026-10-01'),
    (11, 31, 5, '2026-09-14', '2026-09-28'),
    (12, 7, 5, '2026-09-15', '2026-09-29'),
    (12, 36, 3, '2026-09-20', '2026-10-04'),
    (15, 42, 4, '2026-10-01', '2026-10-22'),
    (4, 19, 3, '2026-10-02', '2026-10-16'),
    (4, 21, 3, '2026-10-02', '2026-10-16'),
    (5, 23, 3, '2026-10-05', '2026-10-19'),
    (7, 14, 3, '2026-10-06', '2026-10-20'),
    (9, 12, 3, '2026-10-03', '2026-10-18'),
    (8, 29, 3, '2026-10-07', '2026-10-21'),
    (13, 38, 3, '2026-10-08', '2026-10-22'),
    (14, 40, 3, '2026-10-09', '2026-10-23'),
    (6, 9, 2, '2026-10-11', '2026-10-14'),
    (10, 15, 1, '2026-10-12', '2026-10-15'),
    (11, 30, 6, '2026-10-05', '2026-10-08'),
    (5, 41, 6, '2026-09-25', '2026-09-28'),
    (7, 28, 1, '2026-10-12', '2026-10-15');
GO
