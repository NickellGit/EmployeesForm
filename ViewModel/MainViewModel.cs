using EmployeesForm.Heplers;
using EmployeesForm.Models;
using EmployeesForm.Services;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EmployeesForm.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private readonly DataBaseService _dataBaseService;
        public ObservableCollection<Employee> Employees {  get; set; } = new ObservableCollection<Employee>();

        private Employee _editebleEmployee = new Employee();
        public Employee EditebleEmployee
        {
            get => _editebleEmployee;
            set
            {
                if (value != null)
                {
                    _editebleEmployee = value;
                    OnPropertyChanged();
                }
            }
        }

        private Employee _selectedEmployee;
        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                OnPropertyChanged();

                if (_selectedEmployee != null)
                {
                    EditebleEmployee = new Employee
                    {
                        Id = _selectedEmployee.Id,
                        FirstName = _selectedEmployee.FirstName,
                        SecondName = _selectedEmployee.SecondName,
                        Position = _selectedEmployee.Position,
                        Salary = _selectedEmployee.Salary,
                        HireDate = _selectedEmployee.HireDate,
                        IsRemote = _selectedEmployee.IsRemote,
                    };
                }
                else
                {
                    EditebleEmployee = new Employee();
                }
            }
        }

        public ICommand Save { get; }
        public ICommand Delete { get; }
        public ICommand Clear { get; }

        public MainViewModel()
        {
            _dataBaseService = new DataBaseService(ConfigurationManager.ConnectionStrings["MyDBConnection"].ConnectionString);

            _ = LoadEmployeesAsync();

            Save = new RelayCommand(async _ => await SaveEmployeeChangesAsync(), _ => IsFieldsValid());
            Delete = new RelayCommand(async _ => await DeleteEmployeeAsync(), _ => EditebleEmployee.Id != 0);
            Clear = new RelayCommand(_ => SelectedEmployee = null);
        }

        private async Task LoadEmployeesAsync()
        {
            var loadedEmployees = await _dataBaseService.GetEmployeesAsync();

            Employees.Clear();

            foreach (var employee in loadedEmployees)
            {
                Employees.Add(employee);
            }
        }

        private async Task DeleteEmployeeAsync()
        {
            try
            {
                await _dataBaseService.LogicalDeleteEmployeesAsync(SelectedEmployee.Id);

                Employees.Remove(SelectedEmployee);

                //MessageBox.Show($"Запись \"{SelectedEmployee.SecondName}\" удалена!");
                
                SelectedEmployee = null;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Не удалось удалить запись!\n{e.Message}");
            }
        }

        private async Task SaveEmployeeChangesAsync()
        {
            try
            {
                if (EditebleEmployee.Id == 0)
                {
                    await _dataBaseService.InsertEmployeeAsync(EditebleEmployee);

                    Employees.Add(EditebleEmployee);

                    //MessageBox.Show($"Запись \"{EditebleEmployee.SecondName}\" Добавлена!");
                }
                else
                {
                    await _dataBaseService.UpdateEmployeeAsync(EditebleEmployee);

                    Employees[Employees.IndexOf(SelectedEmployee)] = EditebleEmployee;

                    //MessageBox.Show($"Изменения записи \"{EditebleEmployee.SecondName}\" сохранены!");
                }

                SelectedEmployee = null;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Ошибка при сохранении!\n{e.Message}");
            }
        }

        private bool IsFieldsValid() =>
            EditebleEmployee != null &&
            !string.IsNullOrWhiteSpace(EditebleEmployee.FirstName) &&
            !string.IsNullOrWhiteSpace(EditebleEmployee.SecondName) &&
            !string.IsNullOrWhiteSpace(EditebleEmployee.Position) &&
            EditebleEmployee.HireDate.HasValue;

    }
}
