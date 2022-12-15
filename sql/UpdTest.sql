    update [SpeedLimit]
  set MaxSpeed =60, DeleteFlag = 0 where Position like 'S%'

  --  update [SpeedLimit]
  --set MaxSpeed =60, DeleteFlag = 0 where SegmentID >= 901 and SegmentID <= 1000
  --and Position like 'S%'