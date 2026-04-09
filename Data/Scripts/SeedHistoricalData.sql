-- Historical Data Seeding Script
-- Generates 90 days of rating and view history for all banks
-- Run this script after initial database seeding

SET NOCOUNT ON;

DECLARE @DaysOfHistory INT = 90;
DECLARE @CurrentDate DATE = CAST(GETUTCDATE() AS DATE);
DECLARE @SeedValue INT = 42; -- For reproducibility

-- Check if data already exists
IF EXISTS (SELECT 1 FROM RatingHistories) OR EXISTS (SELECT 1 FROM ViewHistory)
BEGIN
    PRINT 'Historical data already exists. Skipping seeding.';
    RETURN;
END

PRINT 'Starting historical data seeding...';
PRINT 'Days of history: ' + CAST(@DaysOfHistory AS VARCHAR(10));
PRINT '';

-- Temporary table to store bank patterns
CREATE TABLE #BankPatterns (
    BankId INT,
    BankCode NVARCHAR(50),
    Pattern VARCHAR(20),
    BaseViewCount BIGINT
);

-- Insert bank patterns
INSERT INTO #BankPatterns (BankId, BankCode, Pattern, BaseViewCount)
SELECT 
    BankId, 
    BankCode,
    CASE BankCode
        WHEN 'bank-alpha' THEN 'TrendingUp'
        WHEN 'bank-beta' THEN 'TrendingDown'
        WHEN 'bank-gamma' THEN 'Stable'
        WHEN 'bank-delta' THEN 'Volatile'
        WHEN 'bank-epsilon' THEN 'TrendingUp'
        ELSE 'Stable'
    END,
    ViewCount
FROM Banks;

PRINT 'Bank patterns configured:';
SELECT BankCode, Pattern FROM #BankPatterns;
PRINT '';

-- Generate rating history
PRINT 'Generating rating history...';

DECLARE @BankId INT, @CriteriaId INT, @DaysAgo INT;
DECLARE @BankCode NVARCHAR(50), @Pattern VARCHAR(20);
DECLARE @CurrentRating DECIMAL(4,2), @HistoricalRating DECIMAL(4,2);
DECLARE @RecordDate DATE;
DECLARE @Variance DECIMAL(4,2), @DailyNoise DECIMAL(4,2);
DECLARE @RandomSeed INT;

DECLARE bank_cursor CURSOR FOR
SELECT BankId, BankCode, Pattern FROM #BankPatterns;

OPEN bank_cursor;
FETCH NEXT FROM bank_cursor INTO @BankId, @BankCode, @Pattern;

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT 'Processing ' + @BankCode + ' with pattern: ' + @Pattern;
    
    DECLARE criteria_cursor CURSOR FOR
    SELECT CriteriaId FROM RatingCriterias;
    
    OPEN criteria_cursor;
    FETCH NEXT FROM criteria_cursor INTO @CriteriaId;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Get current rating
        SELECT @CurrentRating = RatingValue
        FROM BankRatings
        WHERE BankId = @BankId AND CriteriaId = @CriteriaId;
        
        IF @CurrentRating IS NOT NULL
        BEGIN
            -- Generate daily records
            SET @DaysAgo = @DaysOfHistory - 1;
            
            WHILE @DaysAgo >= 0
            BEGIN
                SET @RecordDate = DATEADD(DAY, -@DaysAgo, @CurrentDate);
                SET @RandomSeed = (@DaysAgo * @CriteriaId * @BankId + @SeedValue) % 1000;
                
                -- Calculate variance based on pattern
                IF @Pattern = 'TrendingUp'
                BEGIN
                    SET @Variance = -(@DaysAgo / CAST(@DaysOfHistory AS DECIMAL(10,2))) * 1.5;
                END
                ELSE IF @Pattern = 'TrendingDown'
                BEGIN
                    SET @Variance = (@DaysAgo / CAST(@DaysOfHistory AS DECIMAL(10,2))) * 1.2;
                END
                ELSE IF @Pattern = 'Stable'
                BEGIN
                    SET @Variance = ((@RandomSeed % 100) / 100.0 - 0.5) * 0.3;
                END
                ELSE -- Volatile
                BEGIN
                    SET @Variance = ((@RandomSeed % 100) / 100.0 - 0.5) * 2.0;
                END
                
                -- Add daily noise
                SET @DailyNoise = (((@DaysAgo + @CriteriaId) % 100) / 100.0 - 0.5) * 0.2;
                
                -- Calculate historical rating
                SET @HistoricalRating = @CurrentRating + @Variance + @DailyNoise;
                
                -- Clamp to valid range (6.0 - 9.5)
                IF @HistoricalRating < 6.0 SET @HistoricalRating = 6.0;
                IF @HistoricalRating > 9.5 SET @HistoricalRating = 9.5;
                
                -- Round to 1 decimal place
                SET @HistoricalRating = ROUND(@HistoricalRating, 1);
                
                -- Insert rating history record
                INSERT INTO RatingHistories (BankId, CriteriaId, OverallRating, RecordedDate)
                VALUES (@BankId, @CriteriaId, @HistoricalRating, @RecordDate);
                
                SET @DaysAgo = @DaysAgo - 1;
            END
        END
        
        FETCH NEXT FROM criteria_cursor INTO @CriteriaId;
    END
    
    CLOSE criteria_cursor;
    DEALLOCATE criteria_cursor;
    
    FETCH NEXT FROM bank_cursor INTO @BankId, @BankCode, @Pattern;
END

CLOSE bank_cursor;
DEALLOCATE bank_cursor;

DECLARE @RatingCount INT;
SELECT @RatingCount = COUNT(*) FROM RatingHistories;
PRINT 'Generated ' + CAST(@RatingCount AS VARCHAR(10)) + ' rating history records';
PRINT '';

-- Generate view history
PRINT 'Generating view history...';

DECLARE @BaseViewCount BIGINT, @HistoricalViewCount INT;
DECLARE @GrowthFactor DECIMAL(10,4), @PatternMultiplier DECIMAL(10,4);
DECLARE @ViewVariance DECIMAL(10,4);

DECLARE view_cursor CURSOR FOR
SELECT BankId, BankCode, Pattern, BaseViewCount FROM #BankPatterns;

OPEN view_cursor;
FETCH NEXT FROM view_cursor INTO @BankId, @BankCode, @Pattern, @BaseViewCount;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @DaysAgo = @DaysOfHistory - 1;
    
    WHILE @DaysAgo >= 0
    BEGIN
        SET @RecordDate = DATEADD(DAY, -@DaysAgo, @CurrentDate);
        SET @RandomSeed = (@DaysAgo * @BankId + @SeedValue) % 100;
        
        -- Calculate growth factor (older dates have fewer views)
        SET @GrowthFactor = 1.0 - (@DaysAgo / CAST(@DaysOfHistory AS DECIMAL(10,4)) * 0.7);
        
        -- Apply pattern-specific multiplier
        IF @Pattern = 'TrendingUp'
        BEGIN
            SET @PatternMultiplier = 1.0 + ((@DaysOfHistory - @DaysAgo) / CAST(@DaysOfHistory AS DECIMAL(10,4)) * 0.3);
        END
        ELSE IF @Pattern = 'TrendingDown'
        BEGIN
            SET @PatternMultiplier = 1.0 - ((@DaysOfHistory - @DaysAgo) / CAST(@DaysOfHistory AS DECIMAL(10,4)) * 0.2);
        END
        ELSE IF @Pattern = 'Volatile'
        BEGIN
            SET @PatternMultiplier = 1.0 + (SIN(@DaysAgo / 7.0) * 0.2);
        END
        ELSE
        BEGIN
            SET @PatternMultiplier = 1.0;
        END
        
        -- Calculate historical view count
        SET @HistoricalViewCount = CAST(@BaseViewCount * @GrowthFactor * @PatternMultiplier AS INT);
        
        -- Add daily variance (±20%)
        SET @ViewVariance = (@RandomSeed / 100.0 - 0.5) * 0.4 + 1.0;
        SET @HistoricalViewCount = CAST(@HistoricalViewCount * @ViewVariance AS INT);
        
        -- Ensure minimum of 1 view per day
        IF @HistoricalViewCount < 1 SET @HistoricalViewCount = 1;
        
        -- Insert view history record
        INSERT INTO ViewHistory (BankId, ViewCount, RecordedDate)
        VALUES (@BankId, @HistoricalViewCount, @RecordDate);
        
        SET @DaysAgo = @DaysAgo - 1;
    END
    
    FETCH NEXT FROM view_cursor INTO @BankId, @BankCode, @Pattern, @BaseViewCount;
END

CLOSE view_cursor;
DEALLOCATE view_cursor;

DECLARE @ViewCount INT;
SELECT @ViewCount = COUNT(*) FROM ViewHistory;
PRINT 'Generated ' + CAST(@ViewCount AS VARCHAR(10)) + ' view history records';
PRINT '';

-- Clean up
DROP TABLE #BankPatterns;

PRINT 'Historical data seeding completed successfully!';
PRINT '';
PRINT 'Summary:';
PRINT '--------';
SELECT 
    B.BankCode,
    COUNT(DISTINCT RH.RecordedDate) AS RatingHistoryDays,
    COUNT(DISTINCT VH.RecordedDate) AS ViewHistoryDays
FROM Banks B
LEFT JOIN RatingHistories RH ON B.BankId = RH.BankId
LEFT JOIN ViewHistory VH ON B.BankId = VH.BankId
GROUP BY B.BankCode
ORDER BY B.BankCode;
