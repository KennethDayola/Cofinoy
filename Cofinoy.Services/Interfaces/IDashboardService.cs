using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofinoy.Services.ServiceModels;

namespace Cofinoy.Services.Interfaces
{
    public interface IDashboardService
    {
        DashboardServiceModel GetDashboardData();
        (decimal today, decimal yesterday, decimal total) GetRevenueStats();
        (int total, int active, int completed, int cancelled) GetOrderStats();
    }
}