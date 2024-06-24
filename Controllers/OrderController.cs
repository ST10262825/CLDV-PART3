using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using JamesCrafts.Models;
using Microsoft.EntityFrameworkCore;
using System;

public class OrderController : Controller
{
    private readonly JamesContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderController> _logger;

    public OrderController(HttpClient httpClient, ILogger<OrderController> logger, JamesContext context)
    {
        _httpClient = httpClient;
        _logger = logger;
        _context = context;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmOrder(int orderId)
    {
        // Fetch order data from your database
        var order = await _context.PurchaseHistories.FindAsync(orderId);
        if (order == null)
        {
            return NotFound();
        }

        // Set order as confirmed
        order.IsConfirmed = true;
        await _context.SaveChangesAsync();

        // Call Azure Function to process order
        var functionUrl = "https://craftsfunctions.azurewebsites.net/api/OrderOrchestrator_HttpStart";
        var json = JsonSerializer.Serialize(order);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(functionUrl, content);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to start order processing orchestration");
            return StatusCode((int)response.StatusCode);
        }

        return RedirectToAction("Index");
    }
}
