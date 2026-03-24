using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyCenterApp.DataAccess.Repositories
{
    /// <summary>
    /// Общий интерфейс репозитория для работы с сущностями
    /// </summary>
    /// <typeparam name="T">Тип сущности</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>Получить все записи</summary>
        List<T> GetAll();

        /// <summary>Получить запись по ID</summary>
        T GetById(int id);

        /// <summary>Добавить новую запись</summary>
        int Add(T entity);

        /// <summary>Обновить запись</summary>
        bool Update(T entity);

        /// <summary>Удалить запись</summary>
        bool Delete(int id);

        /// <summary>Поиск записей по тексту</summary>
        List<T> Search(string searchText);
    }
}