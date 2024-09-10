
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net.WebSockets;
using Trenning_NotificationsExample.Models;

namespace Trenning_NotificationsExample.Services
{
    public class PassportUpdateService 
    {
        private readonly PassportContext _context;
        public PassportUpdateService(PassportContext context)
        {
            _context = context;
        }

        public async Task UpdatePassportsAsync(string filePath)
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            var currentPassports = _context.InactivePassports.ToList();

            var newPassports = new List<InactivePassport>();
            foreach (var line in lines)
            {
                var parts = line.Split(',');// разбили строку на серию и номер соответственно 
                var series = parts[0].Trim();
                var number = parts[1].Trim();

                newPassports.Add(new InactivePassport { Series = series, Number = number, IsActive = true });
            }

            var addedPassports = newPassports.Except(currentPassports, new PassportComparer()).ToList();
            var removedPassports = currentPassports.Except(newPassports, new PassportComparer()).ToList();

            foreach (var passport in addedPassports)
            {
                _context.InactivePassports.Add(passport);
                _context.PassportChanges.Add(new PassportChange
                {
                    Series = passport.Series,
                    Number = passport.Number,
                    ChangeType = "Added",
                    ChangeDate = DateTime.Now
                });
            }

            foreach (var passport in removedPassports)
            {
                passport.IsActive = false;
                _context.PassportChanges.Add(new PassportChange
                {
                    Series = passport.Series,
                    Number = passport.Number,
                    ChangeType = "Removed",
                    ChangeDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
        }
    }

    public class PassportComparer : IEqualityComparer<InactivePassport>
    {
        public bool Equals(InactivePassport x, InactivePassport y)
        {
            return x.Series == y.Series && x.Number == y.Number;
        }

        public int GetHashCode(InactivePassport obj)
        {
            return HashCode.Combine(obj.Series, obj.Number);// Возвращает хэш-код для объекта
        }

    }
}
