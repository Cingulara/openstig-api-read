﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using openstig_read_api.Classes;
using openstig_read_api.Models;
using System.IO;
using System.Text;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.Xml.Serialization;
using System.Xml;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using openstig_read_api.Data;

namespace openstig_read_api.Controllers
{
    //[Route("[controller]")]
    [Route("/")]
    public class ReadController : Controller
    {
	    private readonly IArtifactRepository _artifactRepo;
        private readonly ILogger<ReadController> _logger;

        public ReadController(IArtifactRepository artifactRepo, ILogger<ReadController> logger)
        {
            _logger = logger;
            _artifactRepo = artifactRepo;
        }

        // GET the listing with Ids of the Checklist artifacts, but without all the extra XML
        [HttpGet]
        public async Task<IActionResult> ListArtifacts()
        {
            try {
                IEnumerable<Artifact> artifacts;
                artifacts = await _artifactRepo.GetAllArtifacts();
                foreach (Artifact a in artifacts) {
                    a.rawChecklist = string.Empty;
                }
                return Ok(artifacts);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error listing all artifacts and deserializing the checklist XML");
                return BadRequest();
            }
        }

        // GET /value
        [HttpGet("{id}")]
        public async Task<IActionResult> GetArtifact(string id)
        {
            try {
                Artifact art = new Artifact();
                art = await _artifactRepo.GetArtifact(id);
                art.CHECKLIST = ChecklistLoader.LoadChecklist(art.rawChecklist);
                art.rawChecklist = string.Empty;
                return Ok(art);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error Retrieving Artifact");
                return NotFound();
            }
        }
        
        // GET /value
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadChecklist(string id)
        {
            try {
                Artifact art = new Artifact();
                art = await _artifactRepo.GetArtifact(id);
                return Ok(art.rawChecklist);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error Retrieving Artifact for Download");
                return NotFound();
            }
        }
        
        /******************************************
        * Dashboard Specific API calls
        */
        // GET /count
        [HttpGet("count")]
        public async Task<IActionResult> CountArtifacts(string id)
        {
            try {
                long result = await _artifactRepo.CountChecklists();
                return Ok(result);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error Retrieving Artifact Count in MongoDB");
                return NotFound();
            }
        }
        // GET /latest
        [HttpGet("latest/{number}")]
        public async Task<IActionResult> GetLatestArtifacts(int number)
        {
            try {
                IEnumerable<Artifact> artifacts;
                artifacts = await _artifactRepo.GetLatestArtifacts(number);
                foreach (Artifact a in artifacts) {
                    a.rawChecklist = string.Empty;
                }
                return Ok(artifacts);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error listing latest {0} artifacts and deserializing the checklist XML", number.ToString());
                return BadRequest();
            }
        }

        
        // GET /latest
        [HttpGet("counttype")]
        public async Task<IActionResult> GetCountByType()
        {
            try {
                IEnumerable<Object> artifacts;
                artifacts = await _artifactRepo.GetCountByType();
                return Ok(artifacts);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error getting the counts by type for the Reports page");
                return BadRequest();
            }
        }
    }
}