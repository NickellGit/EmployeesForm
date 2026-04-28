using EmployeesForm.Models;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace EmployeesForm.Services
{
    public class DataBaseService
    {
        private readonly string _connectionString;

        public DataBaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        private async Task<SqlConnection> GetConnectionAsync()
        {
            try
            {
                var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                return connection;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Не удалось подключится к базе данных.\n{ e.Message}");
                throw;
            }
        }
            
        private async Task<ObservableCollection<T>> QueryAsync<T> (string query, Func<SqlDataReader, T> mapping)
        {
            var result = new ObservableCollection<T>();
            try
            {
                using (var connection = await GetConnectionAsync())
                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        try
                        {
                            result.Add(mapping(reader));
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show($"Ошибка маппинга при получении данных.\n{e.Message}");
                            throw;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Ошибка при выполнении запроса: {query.Substring(20)}\n{e.Message}");
                throw;
            }

            return result;
        }
        
        private async Task ExecuteNonQueryAsync(string query, Action<SqlCommand> param)
        {
            try
            {
                using (var connection = await GetConnectionAsync())
                using (var command = new SqlCommand(query, connection))
                {
                    param?.Invoke(command);
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Ошибка при выполнении НЕ запроса: {query.Substring(20)}.\n{e.Message}");
                throw;
            }
        }

        public async Task<ObservableCollection<Employee>> GetEmployeesAsync() =>
            await QueryAsync("SELECT id, firstname, secondname, position, salary, hiredate, isremote FROM dbo.Employees WHERE isdeleted = 0",
                r => new Employee
                {
                    Id = (int)r["id"],
                    FirstName = (string)r["firstname"],
                    SecondName = (string)r["secondname"],
                    Position = (string)r["position"],
                    Salary = SafeGetDecimalOrNull(r, "salary"),
                    HireDate = (DateTime)r["hiredate"],
                    IsRemote = (bool)r["isremote"],
                });

        public async Task LogicalDeleteEmployeesAsync(int id) =>
            await ExecuteNonQueryAsync("UPDATE dbo.Employees SET isdeleted = 1 WHERE id = @id",
                cmd => cmd.Parameters.AddWithValue("id", id));

        public async Task UpdateEmployeeAsync(Employee employee) =>
            await ExecuteNonQueryAsync("UPDATE dbo.Employees SET firstname = @firstname, secondname = @secondname, position = @position, salary = @salary, hiredate = @hiredate, isremote = @isremote WHERE id = @id",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@firstname", employee.FirstName);
                    cmd.Parameters.AddWithValue("@secondname", employee.SecondName);
                    cmd.Parameters.AddWithValue("@position", employee.Position);
                    cmd.Parameters.AddWithValue("@salary", employee.Salary.HasValue ? (object)employee.Salary.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@hiredate", employee.HireDate);
                    cmd.Parameters.AddWithValue("@isremote", employee.IsRemote);
                    cmd.Parameters.AddWithValue("@id", employee.Id);
                });

        public async Task InsertEmployeeAsync(Employee employee)
        {
            var outId = new SqlParameter
            {
                ParameterName = @"outId",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output,
            };

            string query = @"INSERT INTO dbo.Employees (firstname, secondname, position, salary, hiredate, isremote) 
                                    VALUES (@firstname, @secondname, @position, @salary, @hiredate, @isremote);

                             SET @outId = SCOPE_IDENTITY()";

            await ExecuteNonQueryAsync(query,
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@firstname", employee.FirstName);
                    cmd.Parameters.AddWithValue("@secondname", employee.SecondName);
                    cmd.Parameters.AddWithValue("@position", employee.Position);
                    cmd.Parameters.AddWithValue("@salary", employee.Salary.HasValue ? (object)employee.Salary.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@hiredate", employee.HireDate);
                    cmd.Parameters.AddWithValue("@isremote", employee.IsRemote);
                    cmd.Parameters.Add(outId);
                });

            employee.Id = Convert.ToInt32(outId.Value);
        }
            

        private decimal? SafeGetDecimalOrNull(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? (decimal?)null : reader.GetDecimal(ordinal);
        }

    }
}
