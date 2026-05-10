USE ProjectHubDB;
UPDATE Courses SET VideoUrl = 'https://www.youtube.com/embed/Zc0q111k8sA' WHERE Title LIKE '%Angular%';
UPDATE CourseModules SET Content = '<p>Welcome to this course. In this first module, we cover the fundamentals.</p><iframe width=\"100%\" height=\"400\" src=\"https://www.youtube.com/embed/Zc0q111k8sA\" frameborder=\"0\" allowfullscreen></iframe>' WHERE CourseId IN (SELECT Id FROM Courses WHERE Title LIKE '%Angular%') AND OrderIndex = 1;

UPDATE Courses SET VideoUrl = 'https://www.youtube.com/embed/s2gL5G52J-Q' WHERE Title LIKE '%.NET%';
UPDATE CourseModules SET Content = '<p>Welcome to this course. In this first module, we cover the fundamentals.</p><iframe width=\"100%\" height=\"400\" src=\"https://www.youtube.com/embed/s2gL5G52J-Q\" frameborder=\"0\" allowfullscreen></iframe>' WHERE CourseId IN (SELECT Id FROM Courses WHERE Title LIKE '%.NET%') AND OrderIndex = 1;

UPDATE Courses SET VideoUrl = 'https://www.youtube.com/embed/aqz-KE-bpKQ' WHERE Title NOT LIKE '%Angular%' AND Title NOT LIKE '%.NET%';
UPDATE CourseModules SET Content = '<p>Welcome to this course. In this first module, we cover the fundamentals.</p><iframe width=\"100%\" height=\"400\" src=\"https://www.youtube.com/embed/aqz-KE-bpKQ\" frameborder=\"0\" allowfullscreen></iframe>' WHERE CourseId IN (SELECT Id FROM Courses WHERE Title NOT LIKE '%Angular%' AND Title NOT LIKE '%.NET%') AND OrderIndex = 1;
