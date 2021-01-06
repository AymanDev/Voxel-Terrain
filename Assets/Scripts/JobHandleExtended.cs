using Unity.Jobs;

public enum JobHandleStatus {
    Running,
    AwaitingCompletion,
    Completed
}

public struct JobHandleExtended {
    private JobHandle _handle;
    private JobHandleStatus _status;

    public JobHandleExtended(JobHandle handle) : this() {
        _handle = handle;
        _status = JobHandleStatus.Running;
    }

    public JobHandleStatus Status {
        get {
            if (_status == JobHandleStatus.Running && _handle.IsCompleted)
                _status = JobHandleStatus.AwaitingCompletion;

            return _status;
        }
    }

    public void Complete() {
        _handle.Complete();
        _status = JobHandleStatus.Completed;
    }

    public static implicit operator JobHandle(JobHandleExtended extended) {
        return extended._handle;
    }

    public static implicit operator JobHandleExtended(JobHandle handle) {
        return new JobHandleExtended(handle);
    }
}